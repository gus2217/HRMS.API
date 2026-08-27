using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Domain;

/// <summary>A line item on a prescription. References a drug by ID only.</summary>
public sealed class PrescriptionItem : Entity<Guid>
{
    private PrescriptionItem() { } // EF

    internal PrescriptionItem(Guid id, Guid drugId, string dosageInstructions,
        int quantityPrescribed)
        : base(id)
    {
        DrugId = drugId;
        DosageInstructions = dosageInstructions;
        QuantityPrescribed = quantityPrescribed;
        QuantityDispensed = 0;
        Status = PrescriptionItemStatus.Pending;
    }

    public Guid DrugId { get; private set; }
    public string DosageInstructions { get; private set; } = string.Empty;
    public int QuantityPrescribed { get; private set; }
    public int QuantityDispensed { get; private set; }
    public PrescriptionItemStatus Status { get; private set; }

    internal static Result<PrescriptionItem> Create(Guid drugId, string dosageInstructions, int quantityPrescribed)
    {
        if (drugId == Guid.Empty) return Error.Validation("Drug is required.");
        if (string.IsNullOrWhiteSpace(dosageInstructions)) return Error.Validation("Dosage instructions are required.");
        if (quantityPrescribed <= 0) return Error.Validation("Prescribed quantity must be positive.");
        return new PrescriptionItem(Guid.NewGuid(), drugId, dosageInstructions.Trim(), quantityPrescribed);
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
