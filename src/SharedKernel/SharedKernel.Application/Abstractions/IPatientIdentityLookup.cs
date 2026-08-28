namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>Patient display identity (number + full name) for cross-module list views.</summary>
public sealed record PatientIdentityDto(Guid PatientId, string PatientNumber, string FullName);

/// <summary>
/// Resolves patient identities across module schemas. Implemented with a direct
/// read against the patient schema so list endpoints can render patient names
/// without coupling module repositories to the patient DbContext.
/// </summary>
public interface IPatientIdentityLookup
{
    Task<IReadOnlyDictionary<Guid, PatientIdentityDto>> GetIdentitiesAsync(
        IReadOnlyCollection<Guid> patientIds, CancellationToken ct = default);
}
