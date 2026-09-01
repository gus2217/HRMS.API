using Jacana.Inventory.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Application.Features.Inventory;

public sealed record CreateDrugCommand(
    string Code,
    string Name,
    string Category,
    string Form,
    decimal UnitPrice,
    int ReorderLevel)
    : ICommand<Result<DrugCatalogDto>>;

public sealed record UpdateDrugCommand(
    Guid DrugId,
    string Name,
    string Category,
    string Form,
    decimal UnitPrice,
    int ReorderLevel)
    : ICommand<Result<DrugCatalogDto>>;

public sealed record ReceiveStockCommand(
    Guid DrugId,
    string BatchNumber,
    int Quantity,
    DateOnly ExpiryDate,
    decimal UnitCost,
    string? Reference)
    : ICommand<Result<ReceiveStockResponseDto>>;

public sealed record AdjustStockCommand(
    Guid StockBatchId,
    int NewQuantity,
    string? Reason)
    : ICommand<Result<ReceiveStockResponseDto>>;
