using System.Security.Cryptography;
using System.Text;
using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Auth.Refresh;

public sealed class RefreshCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    ITokenService tokens,
    IClock clock)
    : IRequestHandler<RefreshCommand, Result<RefreshTokenResponseDto>>
{
    public async Task<Result<RefreshTokenResponseDto>> Handle(RefreshCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Error.Unauthorized("Refresh token is required.");

        var tokenHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));

        var stored = await refreshTokens.GetByTokenHashAsync(tokenHash, ct);
        if (stored is null || !stored.IsActive(clock.UtcNow))
            return Error.Unauthorized("Refresh token is invalid or expired.");

        var user = await users.GetByIdAsync(stored.UserId, ct);
        if (user is null || user.Status != UserStatus.Active)
            return Error.Unauthorized("User account is not active.");

        var permissionCodes = await users.GetPermissionCodesAsync(user.Id, ct);
        var roleNames = user.Roles.Select(r => r.Role.Name).ToArray();
        var (access, refresh) = tokens.Generate(user.Id, user.FacilityId.Value, roleNames, permissionCodes);

        // Rotate: revoke the old token and link the replacement.
        var replacement = RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refresh))),
            clock.UtcNow.AddDays(7));

        stored.Revoke(replacement.Id);
        await refreshTokens.UpdateAsync(stored, ct);
        await refreshTokens.AddAsync(replacement, ct);

        return new RefreshTokenResponseDto(access, refresh);
    }
}
