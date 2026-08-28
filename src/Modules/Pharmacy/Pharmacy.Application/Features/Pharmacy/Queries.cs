using Jacana.Pharmacy.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Application.Features.Pharmacy;

public sealed record GetPrescriptionQuery(Guid PrescriptionId)
    : IQuery<Result<PrescriptionDetailDto>>;

public sealed record SearchPrescriptionsQuery(int PageNumber, int PageSize, string? Status = null)
    : IQuery<Result<PagedResult<PrescriptionListItemDto>>>;
