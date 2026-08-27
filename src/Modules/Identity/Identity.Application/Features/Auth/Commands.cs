using Jacana.Identity.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Application.Features.Auth;

public sealed record LoginCommand(string Email, string Password, string? TotpCode)
    : ICommand<Result<LoginResponseDto>>;

public sealed record RefreshCommand(string? RefreshToken)
    : ICommand<Result<RefreshTokenResponseDto>>;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Phone,
    string Password,
    IReadOnlyList<string>? RoleNames)
    : ICommand<Result<UserResponseDto>>;

public sealed record AssignRoleCommand(Guid UserId, string RoleName)
    : ICommand<Result<UserResponseDto>>;
