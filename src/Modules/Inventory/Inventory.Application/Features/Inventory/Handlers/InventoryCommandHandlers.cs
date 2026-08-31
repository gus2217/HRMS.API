using Jacana.Inventory.Application.Abstractions;
using Jacana.Inventory.Application.DTOs;
using Jacana.Inventory.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Inventory.Application.Features.Inventory.Handlers;

public sealed class CreateDrugCommandHandler(
    IDrugRepository drugs,
    ICurrentUser currentUser)
    : IRequestHandler<CreateDrugCommand, Result<DrugCatalogDto>>
{
    public async Task<Result<DrugCatalogDto>> Handle(CreateDrugCommand request, CancellationToken ct)
    {
        var price = Money.Create(request.UnitPrice);
        if (price.IsFailure) return price.Error;

        var drug = Drug.Create(Guid.NewGuid(), currentUser.FacilityId,
            request.Code, request.Name, request.Form, price.Value, request.ReorderLevel);
        if (drug.IsFailure) return drug.Error;

        await drugs.AddAsync(drug.Value, ct);

        return new DrugCatalogDto(drug.Value.Id, drug.Value.Code, drug.Value.Name,
            drug.Value.Form, drug.Value.UnitPrice.Amount, drug.Value.ReorderLevel, drug.Value.Status.ToString(), 0);
    }
}

public sealed class ReceiveStockCommandHandler(
    IStockBatchRepository batches,
    IDrugRepository drugs,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<ReceiveStockCommand, Result<ReceiveStockResponseDto>>
{
    public async Task<Result<ReceiveStockResponseDto>> Handle(ReceiveStockCommand request, CancellationToken ct)
    {
        var drug = await drugs.GetByIdAsync(request.DrugId, ct);
        if (drug is null) return Error.NotFound("Drug not found.");

        var cost = Money.Create(request.UnitCost);
        if (cost.IsFailure) return cost.Error;

        var batch = StockBatch.Receive(
            Guid.NewGuid(), currentUser.FacilityId, request.DrugId, request.BatchNumber,
            request.Quantity, request.ExpiryDate, cost.Value, currentUser.UserId, clock.UtcNow);
        if (batch.IsFailure) return batch.Error;

        await batches.AddAsync(batch.Value, ct);

        return new ReceiveStockResponseDto(batch.Value.Id, batch.Value.BatchNumber, batch.Value.QuantityOnHand);
    }
}

public sealed class AdjustStockCommandHandler(
    IStockBatchRepository batches,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<AdjustStockCommand, Result<ReceiveStockResponseDto>>
{
    public async Task<Result<ReceiveStockResponseDto>> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var batch = await batches.GetByIdAsync(request.StockBatchId, ct);
        if (batch is null) return Error.NotFound("Stock batch not found.");

        var result = batch.Adjust(request.NewQuantity, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await batches.UpdateAsync(batch, ct);

        return new ReceiveStockResponseDto(batch.Id, batch.BatchNumber, batch.QuantityOnHand);
    }
}
