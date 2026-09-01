using Jacana.Inpatient.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Application.Features.Inpatient;

public sealed record GetWardsQuery(bool ActiveOnly = false)
    : IQuery<Result<IReadOnlyList<WardDto>>>;

public sealed record GetAdmissionQuery(Guid AdmissionId)
    : IQuery<Result<AdmissionDetailDto>>;

public sealed record SearchAdmissionsQuery(int PageNumber, int PageSize, bool ActiveOnly = true, Guid? PatientId = null)
    : IQuery<Result<PagedResult<AdmissionListItemDto>>>;

public sealed record GetWardOccupancyQuery()
    : IQuery<Result<IReadOnlyList<WardOccupancyDto>>>;
