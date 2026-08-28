using Jacana.Billing.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Application.Features.Billing;

public sealed record GetInvoiceQuery(Guid InvoiceId)
    : IQuery<Result<InvoiceDetailDto>>;

public sealed record SearchInvoicesQuery(int PageNumber, int PageSize, string? Status = null, Guid? ConsultationId = null)
    : IQuery<Result<PagedResult<InvoiceListItemDto>>>;
