namespace Jacana.PatientRegistration.Infrastructure.Services;

/// <summary>
/// Options for the Kenya national Client Registry (CR) — the NUPI/UPI patient
/// registry operated behind the Afya Kenya / DHA partner APIs.
/// Bound from the "ClientRegistry" configuration section.
/// </summary>
public sealed class ClientRegistryOptions
{
    public bool Enabled { get; init; } = true;
    public string CountryCode { get; init; } = "KE";
    public string TokenUrl { get; init; } = string.Empty;
    public string SearchBaseUrl { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 20;
}
