using Jacana.Inventory.Domain;
using Jacana.Inventory.Infrastructure.Persistence;
using Jacana.SharedKernel.Domain;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inventory.Infrastructure.Services;

/// <summary>
/// Write-side stock movement. Deducts dispensed quantities FEFO (first-expiry-first-out)
/// across the drug's usable batches, recording a Dispense movement per batch touched.
/// Consumed by the Pharmacy module's dispense flow — no entity leakage.
/// </summary>
public sealed class InventoryStockService(InventoryDbContext db) : IInventoryStockService
{
    public async Task<Result> DeductAsync(
        Guid drugId,
        int quantity,
        string reference,
        Guid performedByUserId,
        DateTime atUtc,
        CancellationToken ct = default)
    {
        if (quantity <= 0) return Error.Validation("Dispense quantity must be positive.");

        var today = DateOnly.FromDateTime(atUtc);
        var batches = await db.StockBatches
            .Include(b => b.Movements)
            .Where(b => b.DrugId == drugId && b.QuantityOnHand > 0 && b.ExpiryDate >= today)
            .OrderBy(b => b.ExpiryDate) // FEFO
            .ThenBy(b => b.BatchNumber)
            .ToListAsync(ct);

        var available = batches.Sum(b => b.QuantityOnHand);
        if (available < quantity)
            return Error.InvalidOperation($"Insufficient stock: only {available} available.");

        var remaining = quantity;
        foreach (var batch in batches)
        {
            if (remaining <= 0) break;
            var take = Math.Min(remaining, batch.QuantityOnHand);
            var dispense = batch.Dispense(take, reference, performedByUserId, atUtc);
            if (dispense.IsFailure) return dispense.Error;
            // New movements carry client-generated keys — mark them Added so EF emits
            // INSERTs, not phantom UPDATEs (see ChangeTrackingExtensions).
            db.MarkNewChildrenAdded(batch);
            remaining -= take;
        }

        return Result.Success();
    }
}
