using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Domain;

/// <summary>A line item on a prescription. References a drug by ID only.</summary>
public sealed class PrescriptionItem : Entity<Guid>
{
    private PrescriptionItem() { } // EF

    internal PrescriptionItem(Guid id, Guid drugId, string dosageInstructions,
        string route, string frequency, int? durationDays, int quantityPrescribed)
        : base(id)
    {
        DrugId = drugId;
        DosageInstructions = dosageInstructions;
        Route = route;
        Frequency = frequency;
        DurationDays = durationDays;
        QuantityPrescribed = quantityPrescribed;
        QuantityDispensed = 0;
        Status = PrescriptionItemStatus.Pending;
    }

    public Guid DrugId { get; private set; }
    public string DosageInstructions { get; private set; } = string.Empty;
    /// <summary>Administration route (e.g. Oral, IV, Topical).</summary>
    public string Route { get; private set; } = string.Empty;
    /// <summary>Dosing frequency (e.g. "Twice daily", "Every 8 hours").</summary>
    public string Frequency { get; private set; } = string.Empty;
    /// <summary>Course length in days, when applicable (e.g. antibiotics).</summary>
    public int? DurationDays { get; private set; }
    public int QuantityPrescribed { get; private set; }
    public int QuantityDispensed { get; private set; }
    public PrescriptionItemStatus Status { get; private set; }

    internal static Result<PrescriptionItem> Create(
        Guid drugId, string dosageInstructions, string route, string frequency,
        int? durationDays, int quantityPrescribed)
    {
        if (drugId == Guid.Empty) return Error.Validation("Drug is required.");
        if (string.IsNullOrWhiteSpace(dosageInstructions)) return Error.Validation("Dosage instructions are required.");
        if (string.IsNullOrWhiteSpace(route)) return Error.Validation("Route is required.");
        if (quantityPrescribed <= 0) return Error.Validation("Prescribed quantity must be positive.");
        if (durationDays is <= 0) return Error.Validation("Duration must be positive.");
        return new PrescriptionItem(Guid.NewGuid(), drugId, dosageInstructions.Trim(),
            route.Trim(), string.IsNullOrWhiteSpace(frequency) ? string.Empty : frequency.Trim(),
            durationDays, quantityPrescribed);
    }

    public Result Dispense(int quantity)
    {
        if (quantity <= 0) return Error.Validation("Dispense quantity must be positive.");
        if (QuantityDispensed + quantity > QuantityPrescribed)
            return Error.InvalidOperation("Dispense would exceed the prescribed quantity.");

        QuantityDispensed += quantity;
        Status = QuantityDispensed >= QuantityPrescribed
            ? PrescriptionItemStatus.Dispensed
            : PrescriptionItemStatus.PartiallyDispensed;
        return Result.Success();
    }
}
