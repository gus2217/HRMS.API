using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Auth.ChangePassword;

public sealed record ChangePasswordCommand(
    string? Email,
    string? CurrentPassword,
    string NewPassword)
    : ICommand<Result<LoginResponseDto>>;

public sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher hasher,
    ITokenService tokens,
    ITotpService totp,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<ChangePasswordCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var isAuthenticated = currentUser.UserId != Guid.Empty;

        User user;
        if (isAuthenticated)
        {
            user = await users.GetByIdAsync(currentUser.UserId, ct)
                ?? throw new InvalidOperationException("Authenticated user no longer exists.");
        }
        else
        {
            // Anonymous path is reserved for the forced first-login change: the
            // caller must prove the temporary password, and the account must be
            // pending a change. Credentials are verified first so a wrong password
            // or unknown email always reads as Unauthorized (no state leakage).
            // 2FA can never coexist with this path in practice — 2FA enrollment
            // requires a session, and must-change accounts get none.
            if (string.IsNullOrWhiteSpace(request.Email))
                return Error.Unauthorized("Email is required.");

            user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
            if (user is null)
                return Error.Unauthorized("Invalid credentials."); // unknown email ≠ 500

            if (user.Status != UserStatus.Active)
                return Error.Forbidden($"Account is {user.Status.ToString().ToLowerInvariant()}.");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                return Error.Unauthorized("Your current password is required.");

            if (!hasher.Verify(request.CurrentPassword, user.PasswordHash))
                return Error.Unauthorized("Current password is incorrect.");

            if (!user.MustChangePassword)
                return Error.Forbidden("This account is not pending a password change — sign in and change it from your profile.");

            if (user.TwoFactorEnabled)
                return Error.Forbidden("Two-factor is enabled — sign in and change the password from your profile.");
        }

        if (request.NewPassword == request.CurrentPassword)
            return Error.Validation("New password must be different from the current password.");

        var setResult = user.SetPasswordHash(hasher.Hash(request.NewPassword));
        if (setResult.IsFailure) return setResult.Error;

        user.MarkPasswordChanged();

        // Kill every existing session; the response carries a brand-new pair.
        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);

        var permissionCodes = await users.GetPermissionCodesAsync(user.Id, ct);
        var roleNames = user.Roles.Select(r => r.Role.Name).ToArray();
        var (access, refresh) = tokens.Generate(user.Id, user.FacilityId.Value, roleNames, permissionCodes);

        var now = clock.UtcNow;
        await refreshTokens.AddAsync(RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refresh))),
            now.AddDays(7)), ct);

        user.RecordLogin(now);
        await users.UpdateAsync(user, ct);

        return new LoginResponseDto(user.Id, user.FullName, user.Email, roleNames, access, refresh, false, false, permissionCodes);
    }
}
