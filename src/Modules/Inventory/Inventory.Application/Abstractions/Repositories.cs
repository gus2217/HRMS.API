using Jacana.Inventory.Application.DTOs;
using Jacana.Inventory.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;

namespace Jacana.Inventory.Application.Abstractions;

public interface IDrugRepository
{
    Task<Drug?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Drug drug, CancellationToken ct = default);
    Task UpdateAsync(Drug drug, CancellationToken ct = default);
    Task<PagedResult<DrugCatalogDto>> SearchAsync(
        string? search, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<StockLevelDto>> GetStockLevelsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct = default);
}

public interface IStockBatchRepository
{
    Task<StockBatch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StockBatch>> GetByDrugAsync(Guid drugId, CancellationToken ct = default);
    Task AddAsync(StockBatch batch, CancellationToken ct = default);
    Task UpdateAsync(StockBatch batch, CancellationToken ct = default);
}

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier, CancellationToken ct = default);
}
