using Jacana.Laboratory.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Application.Features.Laboratory;

public sealed record GetLabOrderQuery(Guid LabOrderId)
    : IQuery<Result<LabOrderDetailDto>>;
