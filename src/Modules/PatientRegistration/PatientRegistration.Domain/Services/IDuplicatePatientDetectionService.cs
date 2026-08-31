using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Domain;

/// <summary>
/// Domain service contract for duplicate-patient detection. Implemented in
/// Infrastructure (exact phone match, exact NationalId match, and fuzzy name/DOB).
/// Returns candidate matches for staff confirmation — it never silently blocks or merges.
/// </summary>
public interface IDuplicatePatientDetectionService
{
    Task<IReadOnlyList<Patient>> FindDuplicatesAsync(
        FacilityId facilityId,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        PhoneNumber phone,
        NationalId? nationalId,
        CancellationToken ct = default);
}
