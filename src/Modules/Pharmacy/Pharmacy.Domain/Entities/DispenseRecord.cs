using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Domain;

/// <summary>
/// A single dispense event, keyed to a prescription item. Own aggregate because
/// dispensing carries its own audit/idempotency needs distinct from prescribing.
/// </summary>
public sealed class DispenseRecord : AggregateRoot<Guid>
{
    private DispenseRecord() { } // EF

    private DispenseRecord(Guid id, FacilityId facilityId, Guid prescriptionItemId,
        int quantityDispensed, Guid dispensedByUserId, DateTime dispensedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PrescriptionItemId = prescriptionItemId;
        QuantityDispensed = quantityDispensed;
        DispensedByUserId = dispensedByUserId;
        DispensedAtUtc = dispensedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PrescriptionItemId { get; private set; }
    public int QuantityDispensed { get; private set; }
    public Guid DispensedByUserId { get; private set; }
    public DateTime DispensedAtUtc { get; private set; }

    public static Result<DispenseRecord> Create(
        Guid id, FacilityId facilityId, Guid prescriptionItemId,
        int quantityDispensed, Guid dispensedByUserId, DateTime dispensedAtUtc)
    {
        if (prescriptionItemId == Guid.Empty) return Error.Validation("Prescription item is required.");
        if (quantityDispensed <= 0) return Error.Validation("Dispense quantity must be positive.");
        return new DispenseRecord(id, facilityId, prescriptionItemId, quantityDispensed, dispensedByUserId, dispensedAtUtc);
    }
}
