using Jacana.Inventory.Application.Abstractions;
using Jacana.Inventory.Domain;
using Jacana.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

namespace Jacana.Inventory.Infrastructure.Repositories;

public sealed class DrugRepository(InventoryDbContext db) : IDrugRepository
{
    public Task<Drug?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Drugs.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task AddAsync(Drug drug, CancellationToken ct = default)
    {
        db.Drugs.Add(drug);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Drug drug, CancellationToken ct = default)
    {
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still Detached.
        db.MarkNewChildrenAdded(drug);
        return Task.CompletedTask;
    }
}

public sealed class StockBatchRepository(InventoryDbContext db) : IStockBatchRepository
{
    public Task<StockBatch?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.StockBatches.Include(b => b.Movements).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<StockBatch>> GetByDrugAsync(Guid drugId, CancellationToken ct = default)
        => await db.StockBatches.Where(b => b.DrugId == drugId && b.QuantityOnHand > 0).ToListAsync(ct);

    public Task AddAsync(StockBatch batch, CancellationToken ct = default)
    {
        db.StockBatches.Add(batch);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(StockBatch batch, CancellationToken ct = default)
    {
        // Entity already tracked from GetByIdAsync; mutations auto-detected.
        // Never force graph state — it marks new children as Modified (UPDATE 0 rows).
        return Task.CompletedTask;
    }
}

public sealed class SupplierRepository(InventoryDbContext db) : ISupplierRepository
{
    public Task AddAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Suppliers.Add(supplier);
        return Task.CompletedTask;
    }
}
