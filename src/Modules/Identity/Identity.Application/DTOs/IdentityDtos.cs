namespace Jacana.Identity.Application.DTOs;

public sealed record LoginRequestDto(string Email, string Password, string? TotpCode);

/// <summary>
/// For bearer clients the tokens are in the body; for the web client they are set
/// as HttpOnly cookies and these fields are null/empty in the JSON response.
/// </summary>
public sealed record LoginResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor);

public sealed record RefreshTokenRequestDto(string? RefreshToken);

public sealed record RefreshTokenResponseDto(string? AccessToken, string? RefreshToken);

public sealed record RegisterUserRequestDto(
    string FullName,
    string Email,
    string Phone,
    string Password,
    IReadOnlyList<string>? RoleNames);

public sealed record UserResponseDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string Status,
    bool TwoFactorEnabled,
    DateTime? LastLoginAtUtc,
    IReadOnlyList<string> Roles);

public sealed record AssignRoleRequestDto(string RoleName);

public sealed record RoleDto(Guid Id, string Name, IReadOnlyList<string> Permissions);

public sealed record PermissionDto(Guid Id, string Code, string Description);
