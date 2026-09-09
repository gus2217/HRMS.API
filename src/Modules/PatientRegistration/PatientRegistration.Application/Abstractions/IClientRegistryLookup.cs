using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Application.Abstractions;

/// <summary>Demographics returned by the national client registry for a found client.</summary>
public sealed record RegistryClientDto(
    string ClientNumber,      // NUPI
    string? FirstName,
    string? MiddleName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? Gender,           // "Male" / "Female" / null
    string? Phone,            // E.164 when present
    string? County,
    string? SubCounty,
    string? Ward,
    string? Village);

public sealed record RegistryLookupResult(
    bool Found,
    RegistryClientDto? Client,
    string? Message = null);

/// <summary>
/// Lookup against the national client registry (Afya Kenya / DHA partner API).
/// Implementations own OAuth2 client-credentials token acquisition + caching.
/// </summary>
public interface IClientRegistryLookup
{
    /// <summary>Looks up a client by Kenyan National ID. Never throws — network/auth
    /// failures surface as <see cref="RegistryLookupResult"/> with Found=false and a message.</summary>
    Task<RegistryLookupResult> LookupByNationalIdAsync(string nationalId, CancellationToken ct = default);
}
