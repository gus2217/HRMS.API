using Jacana.Laboratory.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Application.Features.Laboratory;

public sealed record GetLabOrderQuery(Guid LabOrderId)
    : IQuery<Result<LabOrderDetailDto>>;

public sealed record SearchLabOrdersQuery(int PageNumber, int PageSize, string? Status = null, Guid? PatientId = null)
    : IQuery<Result<PagedResult<LabOrderListItemDto>>>;
