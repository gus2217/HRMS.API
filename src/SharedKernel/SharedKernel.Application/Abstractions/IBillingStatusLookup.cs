namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Cross-module billing status lookup (raw SQL against the billing schema).
/// Used by the Inpatient module to enforce the "bill cleared before discharge" gate
/// without referencing Billing entities directly.
/// </summary>
public interface IBillingStatusLookup
{
    /// <summary>True when the patient has no outstanding (issued/partially-paid) invoice.</summary>
    Task<bool> IsBillClearedAsync(Guid patientId, CancellationToken ct = default);
}
