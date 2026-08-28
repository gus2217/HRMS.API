using Jacana.Identity.Application;
using Jacana.Inventory.Application.Features.Inventory;
using Jacana.Inventory.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Inventory endpoints: bind → dispatch → map result → return.</summary>
public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inventory");

        group.MapGet("/drugs", GetDrugCatalogAsync)
            .RequireAuthorization(Permissions.Inventory.Receive);

        group.MapPost("/drugs", CreateDrugAsync)
            .RequireAuthorization(Permissions.Inventory.Receive);

        group.MapPost("/stock/receive", ReceiveStockAsync)
            .RequireAuthorization(Permissions.Inventory.Receive);

        group.MapPost("/stock/adjust", AdjustStockAsync)
            .RequireAuthorization(Permissions.Inventory.Adjust);

        group.MapGet("/stock-levels", GetStockLevelsAsync)
            .RequireAuthorization(Permissions.Inventory.Receive);

        group.MapGet("/low-stock", GetLowStockAlertsAsync)
            .RequireAuthorization(Permissions.Inventory.Receive);

        return app;
    }

    private static async Task<IResult> GetDrugCatalogAsync(
        ISender sender, CancellationToken ct, string? search = null, int pageNumber = 1, int pageSize = 100)
    {
        var result = await sender.Send(new GetDrugCatalogQuery(pageNumber, pageSize, search), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CreateDrugAsync(
        CreateDrugRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateDrugCommand(
            request.Code, request.Name, request.Form, request.UnitPrice, request.ReorderLevel), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/inventory/drugs/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ReceiveStockAsync(
        ReceiveStockRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ReceiveStockCommand(
            request.DrugId, request.BatchNumber, request.Quantity,
            request.ExpiryDate, request.UnitCost, request.Reference), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/inventory/stock/{result.Value.StockBatchId}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AdjustStockAsync(
        AdjustStockRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AdjustStockCommand(
            request.StockBatchId, request.NewQuantity, request.Reason), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetStockLevelsAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetStockLevelsQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetLowStockAlertsAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetLowStockAlertsQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.InvalidOperation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
