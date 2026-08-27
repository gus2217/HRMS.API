using Jacana.Inpatient.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Application.Features.Inpatient;

public sealed record GetAdmissionQuery(Guid AdmissionId)
    : IQuery<Result<AdmissionDetailDto>>;

public sealed record GetWardOccupancyQuery()
    : IQuery<Result<IReadOnlyList<WardOccupancyDto>>>;
