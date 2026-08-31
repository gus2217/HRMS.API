namespace Jacana.Inventory.Application.DTOs;

// HTTP request bindings for inventory endpoints.

public sealed record CreateDrugRequestDto(string Code, string Name, string Category, string Form, decimal UnitPrice, int ReorderLevel);

public sealed record ReceiveStockRequestDto(
    Guid DrugId,
    string BatchNumber,
    int Quantity,
    DateOnly ExpiryDate,
    decimal UnitCost,
    string? Reference);

public sealed record AdjustStockRequestDto(Guid StockBatchId, int NewQuantity, string? Reason);
