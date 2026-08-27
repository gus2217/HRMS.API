using Jacana.Identity.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Application.Features.Users;

public sealed record GetUsersQuery(int PageNumber, int PageSize, string? Search)
    : IQuery<Result<PagedResult<UserResponseDto>>>;

public sealed record ListRolesQuery : IQuery<Result<IReadOnlyList<RoleDto>>>;

public sealed record ListPermissionsQuery : IQuery<Result<IReadOnlyList<PermissionDto>>>;
