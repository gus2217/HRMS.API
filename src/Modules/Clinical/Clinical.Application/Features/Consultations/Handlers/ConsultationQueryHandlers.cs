using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
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
