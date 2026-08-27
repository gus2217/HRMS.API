using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>
/// A physical batch of a drug, tracked as its own aggregate so concurrent movements
/// on different batches don't contend on one giant aggregate.
/// </summary>
public sealed class StockBatch : AggregateRoot<Guid>
{
    private readonly List<StockMovement> _movements = new();

    private StockBatch() { } // EF

    private StockBatch(Guid id, FacilityId facilityId, Guid drugId, string batchNumber,
        int quantityOnHand, DateOnly expiryDate, Money unitCost)
        : base(id)
    {
        FacilityId = facilityId;
        DrugId = drugId;
        BatchNumber = batchNumber;
        QuantityOnHand = quantityOnHand;
        ExpiryDate = expiryDate;
        UnitCost = unitCost;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid DrugId { get; private set; }
    public string BatchNumber { get; private set; } = string.Empty;
    public int QuantityOnHand { get; private set; }
    public DateOnly ExpiryDate { get; private set; }
    public Money UnitCost { get; private set; } = null!;

    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    public static Result<StockBatch> Receive(
        Guid id, FacilityId facilityId, Guid drugId, string batchNumber,
        int quantity, DateOnly expiryDate, Money unitCost, Guid performedByUserId, DateTime atUtc)
    {
        if (string.IsNullOrWhiteSpace(batchNumber)) return Error.Validation("Batch number is required.");
        if (quantity <= 0) return Error.Validation("Receipt quantity must be positive.");

        var batch = new StockBatch(id, facilityId, drugId, batchNumber.Trim(), quantity, expiryDate, unitCost);
        batch._movements.Add(StockMovement.Create(StockMovementType.Receipt, quantity, null, performedByUserId, atUtc));
        return batch;
    }

    public Result Dispense(int quantity, string? reference, Guid performedByUserId, DateTime atUtc)
    {
        if (quantity <= 0) return Error.Validation("Dispense quantity must be positive.");
        if (quantity > QuantityOnHand) return Error.InvalidOperation("Insufficient stock in batch.");

        QuantityOnHand -= quantity;
        _movements.Add(StockMovement.Create(StockMovementType.Dispense, quantity, reference, performedByUserId, atUtc));
        return Result.Success();
    }

    public Result Adjust(int newQuantity, Guid performedByUserId, DateTime atUtc)
    {
        if (newQuantity < 0) return Error.Validation("Quantity cannot be negative.");
        var delta = newQuantity - QuantityOnHand;
        if (delta == 0) return Result.Success();

        QuantityOnHand = newQuantity;
        _movements.Add(StockMovement.Create(StockMovementType.Adjustment, Math.Abs(delta), null, performedByUserId, atUtc));
        return Result.Success();
    }
}
