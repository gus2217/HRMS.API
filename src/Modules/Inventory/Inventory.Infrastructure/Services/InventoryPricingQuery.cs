using Jacana.Inventory.Domain;
using Jacana.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inventory.Infrastructure.Services;

/// <summary>
/// Read-only drug pricing lookup. Returns the catalog price (KES) for a drug so
/// the Billing module can price prescription lines without touching Inventory's
/// tables directly.
/// </summary>
public sealed class InventoryPricingQuery(InventoryDbContext db) : IInventoryPricingQuery
{
    public async Task<DrugPriceInfo?> GetPriceAsync(Guid drugId, CancellationToken ct = default)
    {
        var drug = await db.Drugs.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == drugId, ct);

        return drug is null
            ? null
            : new DrugPriceInfo(drug.Id, drug.Code, drug.Name, drug.UnitPrice.Amount);
    }
}
