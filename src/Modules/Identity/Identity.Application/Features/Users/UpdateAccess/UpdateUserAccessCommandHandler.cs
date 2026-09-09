using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users.UpdateAccess;

/// <summary>
/// Replaces a user's role memberships AND direct permission grants in one atomic
/// save. Roles not listed are removed; direct grants not listed are revoked.
/// Effective access = union of role-derived + direct grants.
/// </summary>
public sealed class UpdateUserAccessCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IPermissionRepository permissions)
    : IRequestHandler<UpdateUserAccessCommand, Result<StaffUserDetailDto>>
{
    public async Task<Result<StaffUserDetailDto>> Handle(UpdateUserAccessCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Error.NotFound("User not found.");

        // ── Sync roles (assign missing, remove unlisted) ──────────────────────
        var requestedRoles = request.RoleNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        foreach (var assigned in user.Roles.Select(r => r.Role).ToList())
        {
            if (requestedRoles.Contains(assigned.Name, StringComparer.OrdinalIgnoreCase)) continue;
            var removeResult = user.RemoveRole(assigned.Id);
            if (removeResult.IsFailure) return removeResult.Error;
        }

        foreach (var roleName in requestedRoles)
        {
            if (user.Roles.Any(r => r.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))) continue;

            var role = await roles.GetByNameAsync(roleName, ct);
            if (role is null) return Error.NotFound($"Role '{roleName}' does not exist.");
            user.AssignRole(role);
        }

        // ── Sync direct permission grants ─────────────────────────────────────
        var requestedCodes = request.PermissionCodes.Distinct(StringComparer.Ordinal).ToArray();

        foreach (var direct in user.DirectPermissions.Select(p => p.Permission).ToList())
        {
            if (requestedCodes.Contains(direct.Code, StringComparer.Ordinal)) continue;
            var revokeResult = user.RevokePermission(direct.Id);
            if (revokeResult.IsFailure) return revokeResult.Error;
        }

        foreach (var code in requestedCodes)
        {
            if (user.DirectPermissions.Any(p => p.Permission.Code == code)) continue;

            var permission = await permissions.GetByCodeAsync(code, ct);
            if (permission is null) return Error.NotFound($"Permission '{code}' does not exist.");
            user.GrantPermission(permission);
        }

        await users.UpdateAsync(user, ct);

        // Compute effective access from the in-memory graph — the transaction has
        // not committed yet, so re-querying the DB would return stale grants.
        return StaffUserMapping.ToDetail(user);
    }
}
