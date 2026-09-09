using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Application.Features.Patients;

public sealed record GetPatientQuery(Guid PatientId)
    : IQuery<Result<PatientDetailDto>>;

public sealed record SearchPatientsQuery(string? Search, int PageNumber, int PageSize, string? Sort = null)
    : IQuery<Result<PagedResult<PatientSummaryDto>>>;

/// <summary>Looks a client up in the national client registry by Kenyan National ID.</summary>
public sealed record LookupNationalRegistryQuery(string NationalId)
    : IQuery<Result<RegistryLookupResult>>;
