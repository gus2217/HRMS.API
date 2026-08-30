using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.Consultations.Handlers;

public sealed class GetConsultationQueryHandler(IConsultationRepository consultations)
    : IRequestHandler<GetConsultationQuery, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(GetConsultationQuery request, CancellationToken ct)
    {
        var detail = await consultations.GetDetailAsync(request.ConsultationId, ct);
        return detail is null ? Error.NotFound("Consultation not found.") : detail;
    }
}

public sealed class GetPatientHistoryQueryHandler(IConsultationRepository consultations)
    : IRequestHandler<GetPatientHistoryQuery, Result<PatientClinicalHistoryDto>>
{
    public async Task<Result<PatientClinicalHistoryDto>> Handle(GetPatientHistoryQuery request, CancellationToken ct)
    {
        var history = await consultations.GetPatientHistoryAsync(request.PatientId, ct);
        return history is null ? Error.NotFound("No clinical history found for this patient.") : history;
    }
}

public sealed class GetPatientMedicalRecordQueryHandler(IConsultationRepository consultations)
    : IRequestHandler<GetPatientMedicalRecordQuery, Result<PatientMedicalRecordDto>>
{
    public async Task<Result<PatientMedicalRecordDto>> Handle(GetPatientMedicalRecordQuery request, CancellationToken ct)
    {
        var record = await consultations.GetMedicalRecordAsync(request.PatientId, ct);
        return record is null ? Error.NotFound("No medical record found for this patient.") : record;
    }
}

public sealed class SearchConsultationsQueryHandler(
    IConsultationRepository consultations,
    IPatientIdentityLookup patients)
    : IRequestHandler<SearchConsultationsQuery, Result<PagedResult<ConsultationListItemDto>>>
{
    public async Task<Result<PagedResult<ConsultationListItemDto>>> Handle(
        SearchConsultationsQuery request, CancellationToken ct)
    {
        var items = await consultations.SearchAsync(
            request.Status, request.PageNumber, request.PageSize, ct);
        var total = await consultations.CountAsync(request.Status, ct);

        var identities = await patients.GetIdentitiesAsync(
            items.Select(i => i.PatientId).ToArray(), ct);

        var rows = items.Select(c =>
        {
            identities.TryGetValue(c.PatientId, out var patient);
            return new ConsultationListItemDto(
                c.Id, c.PatientId,
                patient?.PatientNumber ?? string.Empty,
                patient?.FullName ?? string.Empty,
                c.ClinicianUserId, c.Status, c.StartedAtUtc, c.CompletedAtUtc);
        }).ToArray();

        return Result.Success(new PagedResult<ConsultationListItemDto>(
            rows, total, request.PageNumber, request.PageSize));
    }
}
