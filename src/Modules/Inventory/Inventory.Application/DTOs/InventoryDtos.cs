namespace Jacana.Inventory.Application.DTOs;

public sealed record DrugCatalogDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Form,
    decimal UnitPrice,
    int ReorderLevel,
    string Status,
    int AvailableQuantity);

public sealed record StockLevelDto(
    Guid DrugId,
    string DrugCode,
    string DrugName,
    int QuantityOnHand,
    int ReorderLevel);

public sealed record LowStockAlertDto(
    Guid DrugId,
    string DrugCode,
    string DrugName,
    int QuantityOnHand,
    int ReorderLevel);

public sealed record ReceiveStockResponseDto(Guid StockBatchId, string BatchNumber, int QuantityOnHand);

public sealed record SupplierDto(Guid Id, string Name, string? Phone, string? Email);
