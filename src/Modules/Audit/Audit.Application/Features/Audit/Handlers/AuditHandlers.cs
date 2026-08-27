using Jacana.Audit.Application.Abstractions;
using Jacana.Audit.Application.DTOs;
using Jacana.Audit.Application.Features.Audit;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Audit.Application.Features.Audit.Handlers;

public sealed class GetAuditLogQueryHandler(IAuditLogReadRepository auditLog)
    : IRequestHandler<GetAuditLogQuery, Result<PagedResult<AuditLogEntryDto>>>
{
    public async Task<Result<PagedResult<AuditLogEntryDto>>> Handle(GetAuditLogQuery request, CancellationToken ct)
    {
        var items = await auditLog.SearchAsync(request.EntityType, null, request.PageNumber, request.PageSize, ct);
        var total = await auditLog.CountAsync(request.EntityType, ct);
        return new PagedResult<AuditLogEntryDto>(items, total, request.PageNumber, request.PageSize);
    }
}
