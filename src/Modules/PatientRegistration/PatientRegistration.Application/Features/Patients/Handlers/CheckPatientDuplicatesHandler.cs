using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.PatientRegistration.Application.Features.Patients;

/// <summary>
/// Pre-registration check: given a phone and/or NationalId, returns any existing
/// patient that matches exactly. Reception runs this before registering so a
/// duplicate is caught the moment the identifiers are typed — no form submit needed.
/// </summary>
public sealed record CheckPatientDuplicatesQuery(string? Phone, string? NationalId)
    : IQuery<Result<IReadOnlyList<DuplicateCandidateDto>>>;

public sealed class CheckPatientDuplicatesQueryHandler(
    IPatientRepository patients,
    ICurrentUser currentUser)
    : IRequestHandler<CheckPatientDuplicatesQuery, Result<IReadOnlyList<DuplicateCandidateDto>>>
{
    public async Task<Result<IReadOnlyList<DuplicateCandidateDto>>> Handle(
        CheckPatientDuplicatesQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.NationalId))
            return Result.Success<IReadOnlyList<DuplicateCandidateDto>>([]);

        var matches = await patients.FindByPhoneOrNationalIdAsync(
            currentUser.FacilityId, request.Phone, request.NationalId, ct);

        return Result.Success<IReadOnlyList<DuplicateCandidateDto>>(matches.Select(p =>
            new DuplicateCandidateDto(
                p.Id, p.PatientNumber, $"{p.FirstName} {p.LastName}", p.DateOfBirth,
                p.Phone.Value, p.NationalId?.Value)).ToArray());
    }
}
