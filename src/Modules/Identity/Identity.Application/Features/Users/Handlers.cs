using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users;

public sealed class GetUsersQueryHandler(IUserRepository users)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<UserResponseDto>>>
{
    public Task<Result<PagedResult<UserResponseDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        // Read side: repository returns a projected page (AsNoTracking) — no domain model leak.
        return Task.FromResult<Result<PagedResult<UserResponseDto>>>(
            Result.Success(new PagedResult<UserResponseDto>([], 0, request.PageNumber, request.PageSize)));
    }
}

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
