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

public sealed class GetPatientQueryHandler(IPatientRepository patients, ICurrentUser currentUser)
    : IRequestHandler<GetPatientQuery, Result<PatientDetailDto>>
{
    public async Task<Result<PatientDetailDto>> Handle(GetPatientQuery request, CancellationToken ct)
    {
        var detail = await patients.GetDetailAsync(request.PatientId, ct);
        if (detail is null) return Error.NotFound("Patient not found.");

        return currentUser.Permissions.Contains(PatientConfidentialPermission.Code)
            ? detail
            : Masked.Mask(detail);
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
            InsuranceNumber = null,
            County = string.Empty,
            SubCounty = null,
            Ward = null,
            Line1 = null,
            NextOfKin = [],
        };
}
