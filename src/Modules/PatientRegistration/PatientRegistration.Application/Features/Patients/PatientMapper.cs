using Jacana.PatientRegistration.Application.DTOs;
using Jacana.PatientRegistration.Domain;

namespace Jacana.PatientRegistration.Application.Features.Patients;

/// <summary>
/// Maps an in-memory <see cref="Patient"/> aggregate to its detail DTO.
/// Handlers use this after mutation instead of re-querying the database (the
/// unit-of-work transaction has not committed yet at that point).
/// </summary>
internal static class PatientMapper
{
    public static PatientDetailDto ToDetail(Patient p) =>
        new(
            p.Id, p.PatientNumber, p.FirstName, p.LastName, p.DateOfBirth,
            p.Gender.ToString(), p.MaritalStatus.ToString(), p.Phone.Value,
            p.InsuranceType.ToString(), p.InsuranceNumber, p.ClinicType.ToString(),
            p.Address.County, p.Address.SubCounty, p.Address.Ward, p.Address.Line1,
            p.Status.ToString(),
            p.Allergies.Select(a => new AllergyDto(a.Id, a.Substance, a.Severity.ToString(), a.Notes)).ToArray(),
            p.Consents.Select(c => new ConsentDto(c.Type.ToString(), c.Granted, c.RecordedByUserId, null, c.RecordedAtUtc)).ToArray(),
            p.NextOfKin.Select(k => new NextOfKinDto(k.FullName, k.Relationship, k.Phone.Value)).ToArray(),
            p.NationalId?.Value, p.CreatedByUserId, null, p.CreatedAtUtc,
            p.ModifiedByUserId, null, p.ModifiedAtUtc);
}
