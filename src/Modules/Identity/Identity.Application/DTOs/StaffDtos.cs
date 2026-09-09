namespace Jacana.Identity.Application.DTOs;

/// <summary>Row for the staff directory (paged list).</summary>
public sealed record StaffUserListItemDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string Status,
    bool MustChangePassword,
    bool TwoFactorEnabled,
    DateTime? LastLoginAtUtc,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> DirectPermissions);

/// <summary>Full access picture for the staff-management editor.</summary>
public sealed record StaffUserDetailDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string Status,
    bool MustChangePassword,
    bool TwoFactorEnabled,
    DateTime? LastLoginAtUtc,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> DirectPermissions,
    IReadOnlyList<string> EffectivePermissions);

public sealed record CreateStaffRequestDto(
    string FullName,
    string Email,
    string Phone,
    IReadOnlyList<string>? RoleNames,
    IReadOnlyList<string>? PermissionCodes);

/// <summary>
/// Create-staff response. <see cref="TemporaryPassword"/> is the one-time default
/// password — shown to the admin once and never stored or returned again. The
/// account is created with MustChangePassword = true.
/// </summary>
public sealed record CreatedStaffAccountDto(StaffUserDetailDto User, string TemporaryPassword);

public sealed record UpdateUserAccessRequestDto(
    IReadOnlyList<string> RoleNames,
    IReadOnlyList<string> PermissionCodes);

public sealed record ResetPasswordResultDto(string TemporaryPassword);

/// <summary>
/// Change-password request. Two flows share one endpoint:
///  - anonymous forced first-login (Email + CurrentPassword = the temporary one), or
///  - authenticated voluntary change (Email ignored; CurrentPassword = current).
/// </summary>
public sealed record ChangePasswordRequestDto(
    string? Email,
    string? CurrentPassword,
    string NewPassword);
