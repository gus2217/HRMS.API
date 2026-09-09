using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users;

public sealed class ListRolesQueryHandler(IRoleRepository roles)
    : IRequestHandler<ListRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(ListRolesQuery request, CancellationToken ct)
    {
        var all = await roles.GetAllAsync(ct);
        var dtos = all.Select(r => new RoleDto(
            r.Id, r.Name,
            r.Permissions.Select(p => p.Permission.Code).ToArray())).ToArray();
        return dtos;
    }
}

public sealed class ListPermissionsQueryHandler(IPermissionRepository permissions)
    : IRequestHandler<ListPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(ListPermissionsQuery request, CancellationToken ct)
    {
        var all = await permissions.GetAllAsync(ct);
        return all.Select(p => new PermissionDto(p.Id, p.Code, p.Description)).ToArray();
    }
}

/// <summary>Shared mapping: domain User → detail DTO with direct + effective permissions.</summary>
public static class StaffUserMapping
{
    public static StaffUserDetailDto ToDetail(User user, IReadOnlyList<string>? effectivePermissions = null)
    {
        var roles = user.Roles.Select(r => r.Role.Name).OrderBy(n => n).ToArray();
        var direct = user.DirectPermissions.Select(p => p.Permission.Code).OrderBy(c => c).ToArray();
        var effective = effectivePermissions ?? user.Roles
            .SelectMany(r => r.Role.Permissions)
            .Select(rp => rp.Permission.Code)
            .Concat(direct)
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        return new StaffUserDetailDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone.Value,
            user.Status.ToString(),
            user.MustChangePassword,
            user.TwoFactorEnabled,
            user.LastLoginAtUtc,
            roles,
            direct,
            effective);
    }
}
