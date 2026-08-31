using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>Pricing snapshot for a drug, exposed read-only to other modules.</summary>
public sealed record DrugPriceInfo(
    Guid DrugId,
    string Code,
    string Name,
    string Category,
    string Form,
    decimal UnitPrice);

/// <summary>
/// Read-only drug pricing contract (Billing uses it to price prescription lines
/// when auto-billing a consultation). No entity leakage — consumers get a snapshot.
/// </summary>
public interface IInventoryPricingQuery
{
    Task<DrugPriceInfo?> GetPriceAsync(Guid drugId, CancellationToken ct = default);

    /// <summary>Batch price/catalog snapshot for many drugs (one round-trip).</summary>
    Task<IReadOnlyDictionary<Guid, DrugPriceInfo>> GetPricesAsync(
        IReadOnlyCollection<Guid> drugIds, CancellationToken ct = default);
}
