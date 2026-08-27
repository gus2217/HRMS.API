using Jacana.Inventory.Application.Abstractions;
using Jacana.Inventory.Application.DTOs;
using Jacana.Inventory.Domain;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Inventory.Application.Features.Inventory.Handlers;

public sealed class GetDrugCatalogQueryHandler(IDrugRepository drugs)
    : IRequestHandler<GetDrugCatalogQuery, Result<PagedResult<DrugCatalogDto>>>
{
    public Task<Result<PagedResult<DrugCatalogDto>>> Handle(GetDrugCatalogQuery request, CancellationToken ct)
        => Task.FromResult<Result<PagedResult<DrugCatalogDto>>>(
            Result.Success(new PagedResult<DrugCatalogDto>([], 0, request.PageNumber, request.PageSize)));
}

public sealed class GetStockLevelsQueryHandler(IStockBatchRepository batches)
    : IRequestHandler<GetStockLevelsQuery, Result<IReadOnlyList<StockLevelDto>>>
{
    public Task<Result<IReadOnlyList<StockLevelDto>>> Handle(GetStockLevelsQuery request, CancellationToken ct)
        => Task.FromResult<Result<IReadOnlyList<StockLevelDto>>>(Result.Success((IReadOnlyList<StockLevelDto>)[]));
}

public sealed class GetLowStockAlertsQueryHandler(IStockBatchRepository batches)
    : IRequestHandler<GetLowStockAlertsQuery, Result<IReadOnlyList<LowStockAlertDto>>>
{
    public Task<Result<IReadOnlyList<LowStockAlertDto>>> Handle(GetLowStockAlertsQuery request, CancellationToken ct)
        => Task.FromResult<Result<IReadOnlyList<LowStockAlertDto>>>(Result.Success((IReadOnlyList<LowStockAlertDto>)[]));
}
