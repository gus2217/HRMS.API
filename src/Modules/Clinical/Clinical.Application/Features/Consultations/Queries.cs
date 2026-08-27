using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.Consultations;

public sealed record GetConsultationQuery(Guid ConsultationId)
    : IQuery<Result<ConsultationDetailDto>>;

public sealed record GetPatientHistoryQuery(Guid PatientId)
    : IQuery<Result<PatientClinicalHistoryDto>>;
