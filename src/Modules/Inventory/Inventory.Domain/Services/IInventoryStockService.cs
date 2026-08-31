using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>
/// Write contract for deducting stock when medication is dispensed. Exposed to the
/// Pharmacy module so dispensing physically decrements inventory (FEFO across batches)
/// without leaking Inventory's tables or aggregates.
/// </summary>
public interface IInventoryStockService
{
    /// <summary>
    /// Deducts <paramref name="quantity"/> from the drug's non-expired batches (FEFO —
    /// earliest expiry first) and records a Dispense movement per batch. Fails if
    /// there is not enough available stock.
    /// </summary>
    Task<Result> DeductAsync(
        Guid drugId,
        int quantity,
        string reference,
        Guid performedByUserId,
        DateTime atUtc,
        CancellationToken ct = default);
}
