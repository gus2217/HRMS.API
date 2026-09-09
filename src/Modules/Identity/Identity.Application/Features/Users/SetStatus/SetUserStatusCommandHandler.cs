using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users.SetStatus;

public sealed class SetUserStatusCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens)
    : IRequestHandler<SetUserStatusCommand, Result<StaffUserDetailDto>>
{
    public async Task<Result<StaffUserDetailDto>> Handle(SetUserStatusCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Error.NotFound("User not found.");

        if (request.Suspend)
        {
            if (user.Status == UserStatus.Suspended)
                return Error.Validation("Account is already suspended.");
            user.Suspend();
            // Suspending kills every live session immediately.
            await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
        }
        else
        {
            if (user.Status == UserStatus.Active)
                return Error.Validation("Account is already active.");
            user.Reactivate();
        }

        await users.UpdateAsync(user, ct);

        // Status does not change grants — the loaded graph is already authoritative.
        return StaffUserMapping.ToDetail(user);
    }
}
