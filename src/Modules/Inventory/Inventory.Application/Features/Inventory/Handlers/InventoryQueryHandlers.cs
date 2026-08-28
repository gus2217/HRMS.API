using Jacana.Inventory.Application.Abstractions;
using Jacana.Inventory.Application.DTOs;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Inventory.Application.Features.Inventory.Handlers;

public sealed class GetDrugCatalogQueryHandler(IDrugRepository drugs)
    : IRequestHandler<GetDrugCatalogQuery, Result<PagedResult<DrugCatalogDto>>>
{
    public async Task<Result<PagedResult<DrugCatalogDto>>> Handle(
        GetDrugCatalogQuery request, CancellationToken ct)
    {
        var catalog = await drugs.SearchAsync(
            request.Search, request.PageNumber, request.PageSize, ct);
        return Result.Success(catalog);
    }
}

public sealed class GetStockLevelsQueryHandler(IDrugRepository drugs)
    : IRequestHandler<GetStockLevelsQuery, Result<IReadOnlyList<StockLevelDto>>>
{
    public async Task<Result<IReadOnlyList<StockLevelDto>>> Handle(
        GetStockLevelsQuery request, CancellationToken ct)
    {
        var levels = await drugs.GetStockLevelsAsync(ct);
        return Result.Success(levels);
    }
}

public sealed class GetLowStockAlertsQueryHandler(IDrugRepository drugs)
    : IRequestHandler<GetLowStockAlertsQuery, Result<IReadOnlyList<LowStockAlertDto>>>
{
    public async Task<Result<IReadOnlyList<LowStockAlertDto>>> Handle(
        GetLowStockAlertsQuery request, CancellationToken ct)
    {
        var alerts = await drugs.GetLowStockAlertsAsync(ct);
        return Result.Success(alerts);
    }
}
