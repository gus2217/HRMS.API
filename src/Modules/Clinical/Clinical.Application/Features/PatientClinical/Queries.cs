using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.PatientClinical;

public sealed record GetVitalsQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<VitalSignDto>>>;

public sealed record GetImmunizationsQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<ImmunizationDto>>>;

public sealed record GetConditionsQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<ConditionDto>>>;
