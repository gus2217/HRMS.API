namespace Jacana.PatientRegistration.Application.Abstractions;

using Jacana.PatientRegistration.Application.DTOs;
using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Domain;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Patient?> GetByPatientNumberAsync(string patientNumber, CancellationToken ct = default);
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task UpdateAsync(Patient patient, CancellationToken ct = default);
    Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(
        string? search, int pageNumber, int pageSize, string? sort = null, CancellationToken ct = default);
    Task<int> CountAsync(string? search, CancellationToken ct = default);
    Task<PatientDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Exact-match lookup by phone and/or NationalId within a facility — the
    /// reception pre-registration check. Returns empty when nothing matches.
    /// </summary>
    Task<IReadOnlyList<Patient>> FindByPhoneOrNationalIdAsync(
        FacilityId facilityId, string? phone, string? nationalId, CancellationToken ct = default);
}
