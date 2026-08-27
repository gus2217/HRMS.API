using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Inpatient.Application.Features.Inpatient.Handlers;

public sealed class AdmitPatientCommandHandler(
    IAdmissionRepository admissions,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<AdmitPatientCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(AdmitPatientCommand request, CancellationToken ct)
    {
        var admission = Admission.Admit(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId,
            request.AdmittingClinicianUserId, request.WardName, request.BedNumber, clock.UtcNow);
        if (admission.IsFailure) return admission.Error;

        await admissions.AddAsync(admission.Value, ct);
        // Map from the in-memory aggregate — the unit-of-work transaction has not
        // committed yet, so a re-query would not see the new row.
        return AdmissionMapper.ToDetail(admission.Value);
    }
}

public sealed class DischargePatientCommandHandler(
    IAdmissionRepository admissions,
    IClock clock)
    : IRequestHandler<DischargePatientCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(DischargePatientCommand request, CancellationToken ct)
    {
        var admission = await admissions.GetByIdAsync(request.AdmissionId, ct);
        if (admission is null) return Error.NotFound("Admission not found.");

        var result = admission.Discharge(clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await admissions.UpdateAsync(admission, ct);
        return AdmissionMapper.ToDetail(admission);
    }
}

public sealed class AddWardNoteCommandHandler(
    IAdmissionRepository admissions,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<AddWardNoteCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(AddWardNoteCommand request, CancellationToken ct)
    {
        var admission = await admissions.GetByIdAsync(request.AdmissionId, ct);
        if (admission is null) return Error.NotFound("Admission not found.");

        var result = admission.AddWardNote(request.Content, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await admissions.UpdateAsync(admission, ct);
        return AdmissionMapper.ToDetail(admission);
    }
}

public sealed class GetAdmissionQueryHandler(IAdmissionRepository admissions)
    : IRequestHandler<GetAdmissionQuery, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(GetAdmissionQuery request, CancellationToken ct)
    {
        var detail = await admissions.GetDetailAsync(request.AdmissionId, ct);
        return detail is null ? Error.NotFound("Admission not found.") : detail;
    }
}

public sealed class GetWardOccupancyQueryHandler(IAdmissionRepository admissions)
    : IRequestHandler<GetWardOccupancyQuery, Result<IReadOnlyList<WardOccupancyDto>>>
{
    public async Task<Result<IReadOnlyList<WardOccupancyDto>>> Handle(GetWardOccupancyQuery request, CancellationToken ct)
    {
        var occupancy = await admissions.GetWardOccupancyAsync(ct);
        return Result.Success(occupancy);
    }
}
