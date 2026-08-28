using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Domain;

/// <summary>
/// A prescription (order to dispense). References the originating consultation by ID.
/// </summary>
public sealed class Prescription : AggregateRoot<Guid>
{
    private readonly List<PrescriptionItem> _items = new();

    private Prescription() { } // EF

    private Prescription(Guid id, FacilityId facilityId, Guid patientId, Guid consultationId,
        Guid prescribedByUserId, DateTime prescribedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ConsultationId = consultationId;
        PrescribedByUserId = prescribedByUserId;
        PrescribedAtUtc = prescribedAtUtc;
        Status = PrescriptionStatus.Pending;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid ConsultationId { get; private set; }
    public Guid PrescribedByUserId { get; private set; }
    public DateTime PrescribedAtUtc { get; private set; }
    public PrescriptionStatus Status { get; private set; }

    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    public static Result<Prescription> Create(
        Guid id, FacilityId facilityId, Guid patientId, Guid consultationId,
        Guid prescribedByUserId, DateTime prescribedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (prescribedByUserId == Guid.Empty) return Error.Validation("Prescriber is required.");
        return new Prescription(id, facilityId, patientId, consultationId, prescribedByUserId, prescribedAtUtc);
    }

    public Result AddItem(Guid drugId, string dosageInstructions, int quantityPrescribed)
    {
        var item = PrescriptionItem.Create(drugId, dosageInstructions, quantityPrescribed);
        if (item.IsFailure) return item.Error;
        _items.Add(item.Value);
        return Result.Success();
    }

    /// <summary>
    /// Publishes <see cref="PrescriptionCreatedDomainEvent"/> (delivered via the outbox
    /// to the Billing module for auto-billing). Call after all items are attached.
    /// </summary>
    public void PublishCreated()
        => AddDomainEvent(new PrescriptionCreatedDomainEvent(
            Id, FacilityId.Value, PatientId, ConsultationId,
            _items.Select(i => new PrescriptionItemData(
                i.DrugId, i.DosageInstructions, i.QuantityPrescribed)).ToArray(),
            PrescribedAtUtc));

    public Result DispenseItem(Guid itemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return Error.NotFound("Prescription item not found.");

        var result = item.Dispense(quantity);
        if (result.IsFailure) return result.Error;

        RecomputeStatus();
        return Result.Success();
    }

    private void RecomputeStatus()
    {
        if (_items.All(i => i.Status == PrescriptionItemStatus.Dispensed))
            Status = PrescriptionStatus.FullyDispensed;
        else if (_items.Any(i => i.Status == PrescriptionItemStatus.Dispensed
            || i.Status == PrescriptionItemStatus.PartiallyDispensed))
            Status = PrescriptionStatus.PartiallyDispensed;
        else
            Status = PrescriptionStatus.Pending;
    }
}
