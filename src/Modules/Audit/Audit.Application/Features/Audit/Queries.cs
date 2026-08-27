using Jacana.Audit.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Audit.Application.Features.Audit;

public sealed record GetAuditLogQuery(
    string? EntityType,
    int PageNumber,
    int PageSize)
    : IQuery<Result<PagedResult<AuditLogEntryDto>>>;
