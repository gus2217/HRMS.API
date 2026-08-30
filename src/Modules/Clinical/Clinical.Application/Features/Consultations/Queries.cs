using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.Consultations;

public sealed record GetConsultationQuery(Guid ConsultationId)
    : IQuery<Result<ConsultationDetailDto>>;

public sealed record SearchConsultationsQuery(int PageNumber, int PageSize, string? Status = null)
    : IQuery<Result<PagedResult<ConsultationListItemDto>>>;

public sealed record GetPatientHistoryQuery(Guid PatientId)
    : IQuery<Result<PatientClinicalHistoryDto>>;

public sealed record GetPatientMedicalRecordQuery(Guid PatientId)
    : IQuery<Result<PatientMedicalRecordDto>>;
