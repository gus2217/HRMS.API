using Jacana.Billing.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Application.Features.Billing;

public sealed record GetInvoiceQuery(Guid InvoiceId)
    : IQuery<Result<InvoiceDetailDto>>;
