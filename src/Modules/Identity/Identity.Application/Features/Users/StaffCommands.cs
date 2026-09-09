using Jacana.Identity.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Application.Features.Users;

public sealed record GetUsersQuery(int PageNumber, int PageSize, string? Search)
    : IQuery<Result<PagedResult<StaffUserListItemDto>>>;

public sealed record GetUserDetailQuery(Guid UserId)
    : IQuery<Result<StaffUserDetailDto>>;

public sealed record CreateStaffUserCommand(
    string FullName,
    string Email,
    string Phone,
    IReadOnlyList<string> RoleNames,
    IReadOnlyList<string> PermissionCodes)
    : ICommand<Result<CreatedStaffAccountDto>>;

/// <summary>Replaces the user's roles AND direct permission grants in one call.</summary>
public sealed record UpdateUserAccessCommand(
    Guid UserId,
    IReadOnlyList<string> RoleNames,
    IReadOnlyList<string> PermissionCodes)
    : ICommand<Result<StaffUserDetailDto>>;

public sealed record SetUserStatusCommand(Guid UserId, bool Suspend)
    : ICommand<Result<StaffUserDetailDto>>;

/// <summary>Admin reset: issues a new temporary password and forces a change at next login.</summary>
public sealed record ResetUserPasswordCommand(Guid UserId)
    : ICommand<Result<ResetPasswordResultDto>>;
