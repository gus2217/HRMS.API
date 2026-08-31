using Jacana.Audit.Application.Abstractions;
using Jacana.Audit.Application.DTOs;
using Jacana.Audit.Application.Features.Audit;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Audit.Application.Features.Audit.Handlers;

public sealed class GetAuditLogQueryHandler(
    IAuditLogReadRepository auditLog,
    IPatientIdentityLookup patients,
    IUserIdentityLookup users)
    : IRequestHandler<GetAuditLogQuery, Result<PagedResult<AuditLogEntryDto>>>
{
    public async Task<Result<PagedResult<AuditLogEntryDto>>> Handle(GetAuditLogQuery request, CancellationToken ct)
    {
        var items = await auditLog.SearchAsync(request.EntityType, null, request.PageNumber, request.PageSize, ct);
        var total = await auditLog.CountAsync(request.EntityType, ct);

        // Resolve names for performers AND for User-typed entities (both live in the
        // identity schema). Guid.Empty is the unauthenticated/system seed marker.
        var userIdsToResolve = items
            .Select(i => i.PerformedByUserId)
            .Concat(items
                .Where(i => i.EntityType == "User")
                .Select(i => Guid.TryParse(i.EntityId, out var g) ? g : Guid.Empty))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
        var userNames = await users.GetIdentitiesAsync(userIdsToResolve, ct);

        // Resolve entity names for human-facing entity types (Patient, User).
        var patientIds = items
            .Where(i => i.EntityType == "Patient")
            .Select(i => Guid.TryParse(i.EntityId, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .Distinct()
            .ToArray();
        var patientNames = patientIds.Length > 0
            ? await patients.GetIdentitiesAsync(patientIds, ct)
            : new Dictionary<Guid, PatientIdentityDto>();

        var enriched = items.Select(i =>
        {
            var performerName = i.PerformedByUserId == Guid.Empty
                ? "System"
                : userNames.TryGetValue(i.PerformedByUserId, out var performer) ? performer.FullName : null;
            var entityName = i.EntityType switch
            {
                "Patient" when Guid.TryParse(i.EntityId, out var pid) && patientNames.TryGetValue(pid, out var p)
                    => p.FullName,
                "User" when Guid.TryParse(i.EntityId, out var uid) && userNames.TryGetValue(uid, out var u)
                    => u.FullName,
                _ => null
            };
            return i with { PerformedByName = performerName, EntityName = entityName };
        }).ToArray();

        return new PagedResult<AuditLogEntryDto>(enriched, total, request.PageNumber, request.PageSize);
    }
}
