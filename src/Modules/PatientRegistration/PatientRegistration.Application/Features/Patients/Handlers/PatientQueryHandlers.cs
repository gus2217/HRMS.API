using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.PatientRegistration.Application.Features.Patients;

/// <summary>
/// Permission code that unlocks confidential patient data (phone, SHA number,
/// address, next of kin). Mirrors Jacana.Identity.Application.Permissions.Patients.
/// Roles that only identify patients (Lab Technician, Pharmacist) hold
/// Patient.View without it and receive masked records.
/// </summary>
internal static class PatientConfidentialPermission
{
    public const string Code = "Patient.ConfidentialView";
}

public sealed class GetPatientQueryHandler(
    IPatientRepository patients,
    ICurrentUser currentUser,
    IUserIdentityLookup users)
    : IRequestHandler<GetPatientQuery, Result<PatientDetailDto>>
{
    public async Task<Result<PatientDetailDto>> Handle(GetPatientQuery request, CancellationToken ct)
    {
        var detail = await patients.GetDetailAsync(request.PatientId, ct);
        if (detail is null) return Error.NotFound("Patient not found.");

        var enriched = await EnrichNamesAsync(detail, ct);
        return currentUser.Permissions.Contains(PatientConfidentialPermission.Code)
            ? enriched
            : Masked.Mask(enriched);
    }

    /// <summary>
    /// Resolves display names for the staff who created/updated the record and
    /// recorded each consent — so the record shows who did what, when.
    /// </summary>
    private async Task<PatientDetailDto> EnrichNamesAsync(PatientDetailDto d, CancellationToken ct)
    {
        var ids = new List<Guid>();
        if (d.CreatedByUserId != Guid.Empty) ids.Add(d.CreatedByUserId);
        if (d.ModifiedByUserId is { } m && m != Guid.Empty) ids.Add(m);
        ids.AddRange(d.Consents.Select(c => c.RecordedByUserId).Where(id => id != Guid.Empty));
        if (ids.Count == 0) return d;

        var names = await users.GetIdentitiesAsync(ids.Distinct().ToArray(), ct);
        string? NameOf(Guid id) => names.TryGetValue(id, out var u) ? u.FullName : null;

        return d with
        {
            CreatedByName = NameOf(d.CreatedByUserId),
            ModifiedByName = d.ModifiedByUserId is { } mu ? NameOf(mu) : null,
            Consents = d.Consents
                .Select(c => c with { RecordedByName = NameOf(c.RecordedByUserId) })
                .ToArray(),
        };
    }
}

public sealed class SearchPatientsQueryHandler(IPatientRepository patients, ICurrentUser currentUser)
    : IRequestHandler<SearchPatientsQuery, Result<PagedResult<PatientSummaryDto>>>
{
    public async Task<Result<PagedResult<PatientSummaryDto>>> Handle(SearchPatientsQuery request, CancellationToken ct)
    {
        var items = await patients.SearchAsync(request.Search, request.PageNumber, request.PageSize, request.Sort, ct);
        var total = await patients.CountAsync(request.Search, ct);

        if (!currentUser.Permissions.Contains(PatientConfidentialPermission.Code))
            items = items.Select(Masked.Mask).ToArray();

        return new PagedResult<PatientSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}

/// <summary>
/// Masks confidential fields for roles that only need to identify a patient
/// (lab results, dispensing) — contact details, SHA number, address and next of
/// kin are withheld; clinical identity (name, DOB, allergies, consents) stays.
/// </summary>
internal static class Masked
{
    public static PatientSummaryDto Mask(PatientSummaryDto s)
        => s with { Phone = null };

    public static PatientDetailDto Mask(PatientDetailDto d)
        => d with
        {
            Phone = null,
            NationalId = null,
            InsuranceNumber = null,
            County = string.Empty,
            SubCounty = null,
            Ward = null,
            Line1 = null,
            NextOfKin = [],
        };
}
