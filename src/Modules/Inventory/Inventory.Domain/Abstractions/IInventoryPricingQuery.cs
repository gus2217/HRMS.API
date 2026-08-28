using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>Pricing snapshot for a drug, exposed read-only to other modules.</summary>
public sealed record DrugPriceInfo(Guid DrugId, string Code, string Name, decimal UnitPrice);

/// <summary>
/// Read-only drug pricing contract (Billing uses it to price prescription lines
/// when auto-billing a consultation). No entity leakage — consumers get a snapshot.
/// </summary>
public interface IInventoryPricingQuery
{
    Task<DrugPriceInfo?> GetPriceAsync(Guid drugId, CancellationToken ct = default);
}
