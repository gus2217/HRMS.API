using Jacana.Inventory.Domain;
using Jacana.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inventory.Infrastructure.Services;

/// <summary>
/// Read-only stock query. Sums quantity-on-hand across non-expired batches for a drug.
/// Consumed by the Pharmacy module's dispense guard — no entity leakage across modules.
/// </summary>
public sealed class InventoryStockQuery(InventoryDbContext db) : IInventoryStockQuery
{
    public async Task<int> GetAvailableQuantityAsync(Guid drugId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.StockBatches
            .Where(b => b.DrugId == drugId && b.QuantityOnHand > 0 && b.ExpiryDate >= today)
            .SumAsync(b => b.QuantityOnHand, ct);
    }

    public Task<bool> IsTrackedAsync(Guid drugId, CancellationToken ct = default)
        => db.StockBatches.AnyAsync(b => b.DrugId == drugId, ct);
}
