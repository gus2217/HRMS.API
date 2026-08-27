namespace Jacana.Inventory.Domain;

public enum StockMovementType
{
    Receipt,
    Dispense,
    Adjustment,
    Return,
    ExpiryWriteOff,
    TransferOut,
    TransferIn
}
