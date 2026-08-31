using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>
/// Read-only stock query contract exposed to the Pharmacy module. No entity leakage —
/// Pharmacy asks "how much of drug X is available?" without touching Inventory's tables.
/// </summary>
public interface IInventoryStockQuery
{
    Task<int> GetAvailableQuantityAsync(Guid drugId, CancellationToken ct = default);

    /// <summary>
    /// True when the drug has at least one stock batch recorded (i.e. inventory
    /// is actually tracked for it). Pharmacy uses this to decide whether the
    /// dispense stock guard applies: an untracked drug must not block dispensing.
    /// </summary>
    Task<bool> IsTrackedAsync(Guid drugId, CancellationToken ct = default);
}
