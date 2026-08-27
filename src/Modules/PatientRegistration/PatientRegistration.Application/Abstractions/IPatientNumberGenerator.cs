namespace Jacana.PatientRegistration.Application.Abstractions;

using Jacana.SharedKernel.Domain;

/// <summary>Generates facility-scoped patient numbers (e.g. "PT-000123").</summary>
public interface IPatientNumberGenerator
{
    Task<string> NextAsync(FacilityId facilityId, CancellationToken ct = default);
}
