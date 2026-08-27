using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher hasher,
    ITokenService tokens,
    ITotpService totp,
    IClock clock)
    : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null)
            return Error.Unauthorized("Invalid credentials.");

        if (user.Status != UserStatus.Active)
            return Error.Forbidden($"Account is {user.Status.ToString().ToLowerInvariant()}.");

        if (!hasher.Verify(request.Password, user.PasswordHash))
            return Error.Unauthorized("Invalid credentials.");

        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.TotpCode))
                return new LoginResponseDto(user.Id, user.FullName, user.Email, [], null, null, true);

            if (!totp.Validate(user.TotpSecret!, request.TotpCode))
                return Error.Unauthorized("Invalid two-factor code.");
        }

        var permissionCodes = await users.GetPermissionCodesAsync(user.Id, ct);
        var roleNames = user.Roles.Select(r => r.Role.Name).ToArray();
        var (access, refresh) = tokens.Generate(user.Id, user.FacilityId.Value, roleNames, permissionCodes);

        var now = clock.UtcNow;
        refreshTokens.AddAsync(RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refresh))),
            now.AddDays(7)), ct);

        user.RecordLogin(now);
        await users.UpdateAsync(user, ct);

        return new LoginResponseDto(user.Id, user.FullName, user.Email, roleNames, access, refresh, false);
    }
}
