using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>
/// Read-only stock query contract exposed to the Pharmacy module. No entity leakage —
/// Pharmacy asks "how much of drug X is available?" without touching Inventory's tables.
/// </summary>
public interface IInventoryStockQuery
{
    Task<int> GetAvailableQuantityAsync(Guid drugId, CancellationToken ct = default);
}
