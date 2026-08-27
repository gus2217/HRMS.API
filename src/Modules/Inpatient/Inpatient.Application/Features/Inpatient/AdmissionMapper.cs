using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;

namespace Jacana.Inpatient.Application.Features.Inpatient;

/// <summary>
/// Maps an in-memory <see cref="Admission"/> aggregate to its detail DTO.
/// Handlers use this after mutation instead of re-querying the database (the
/// unit-of-work transaction has not committed yet at that point).
/// </summary>
internal static class AdmissionMapper
{
    public static AdmissionDetailDto ToDetail(Admission a) =>
        new(
            a.Id, a.PatientId, a.AdmittingClinicianUserId, a.WardName, a.BedNumber,
            a.Status.ToString(), a.AdmittedAtUtc, a.DischargedAtUtc,
            a.Notes.Select(n => new WardNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc)).ToArray());
}
