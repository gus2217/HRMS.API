using Jacana.Inpatient.Application.DTOs;
using Jacana.SharedKernel.Application.Abstractions;

namespace Jacana.Inpatient.Application.Features.Inpatient;

/// <summary>
/// Resolves display names for every staff member referenced by an admission
/// record (admitting/attending clinician, note authors, medical-record
/// recorders, attachment uploaders) so the ward view shows who did what.
/// </summary>
internal static class AdmissionDetailEnricher
{
    public static async Task<AdmissionDetailDto> EnrichAsync(
        AdmissionDetailDto dto, IUserIdentityLookup users, CancellationToken ct)
    {
        var ids = new List<Guid>();
        if (dto.AdmittingClinicianUserId != Guid.Empty) ids.Add(dto.AdmittingClinicianUserId);
        if (dto.AttendingClinicianUserId is { } attending) ids.Add(attending);
        ids.AddRange(dto.Notes.Select(n => n.AuthorUserId));
        ids.AddRange(dto.MedicalRecords.Select(r => r.RecordedByUserId));
        ids.AddRange(dto.MedicalRecords.SelectMany(r => r.Attachments).Select(a => a.UploadedByUserId));

        var map = await users.GetIdentitiesAsync(ids.Distinct().ToArray(), ct);
        string? NameOf(Guid id) => map.TryGetValue(id, out var u) ? u.FullName : null;

        return dto with
        {
            AdmittingClinicianName = NameOf(dto.AdmittingClinicianUserId),
            AttendingClinicianName = dto.AttendingClinicianUserId is { } a ? NameOf(a) : null,
            Notes = dto.Notes
                .Select(n => n with { AuthorName = NameOf(n.AuthorUserId) })
                .ToArray(),
            MedicalRecords = dto.MedicalRecords
                .Select(r => r with
                {
                    RecordedByName = NameOf(r.RecordedByUserId),
                    Attachments = r.Attachments
                        .Select(a => a with { UploadedByName = NameOf(a.UploadedByUserId) })
                        .ToArray(),
                })
                .ToArray(),
        };
    }
}
