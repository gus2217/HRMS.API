using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Application.Features.Patients;

public sealed record GetPatientQuery(Guid PatientId)
    : IQuery<Result<PatientDetailDto>>;

public sealed record SearchPatientsQuery(string? Search, int PageNumber, int PageSize, string? Sort = null)
    : IQuery<Result<PagedResult<PatientSummaryDto>>>;
