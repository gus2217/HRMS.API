using Jacana.Identity.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Application.Features.Users;

public sealed record ListRolesQuery : IQuery<Result<IReadOnlyList<RoleDto>>>;

public sealed record ListPermissionsQuery : IQuery<Result<IReadOnlyList<PermissionDto>>>;
