using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users.ResetPassword;

/// <summary>
/// Admin-initiated reset. Generates a fresh temporary password, forces a change
/// at next login, and revokes all existing refresh tokens so current sessions die.
/// The temporary password is returned once — the same contract as account creation.
/// </summary>
public sealed class ResetUserPasswordCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher hasher,
    IDefaultPasswordGenerator passwordGenerator)
    : IRequestHandler<ResetUserPasswordCommand, Result<ResetPasswordResultDto>>
{
    public async Task<Result<ResetPasswordResultDto>> Handle(ResetUserPasswordCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Error.NotFound("User not found.");

        var temporaryPassword = passwordGenerator.Generate();
        var setResult = user.SetPasswordHash(hasher.Hash(temporaryPassword));
        if (setResult.IsFailure) return setResult.Error;

        user.RequirePasswordChange();
        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
        await users.UpdateAsync(user, ct);

        return new ResetPasswordResultDto(temporaryPassword);
    }
}
