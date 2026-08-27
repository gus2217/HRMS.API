using Jacana.Inventory.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Application.Features.Inventory;

public sealed record GetDrugCatalogQuery(int PageNumber, int PageSize, string? Search)
    : IQuery<Result<PagedResult<DrugCatalogDto>>>;

public sealed record GetStockLevelsQuery()
    : IQuery<Result<IReadOnlyList<StockLevelDto>>>;

public sealed record GetLowStockAlertsQuery()
    : IQuery<Result<IReadOnlyList<LowStockAlertDto>>>;
