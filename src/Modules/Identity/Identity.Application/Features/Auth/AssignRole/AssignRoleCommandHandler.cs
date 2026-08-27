using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Auth.AssignRole;

public sealed class AssignRoleCommandHandler(
    IUserRepository users,
    IRoleRepository roles)
    : IRequestHandler<AssignRoleCommand, Result<UserResponseDto>>
{
    public async Task<Result<UserResponseDto>> Handle(AssignRoleCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Error.NotFound("User not found.");

        var role = await roles.GetByNameAsync(request.RoleName, ct);
        if (role is null) return Error.NotFound($"Role '{request.RoleName}' does not exist.");

        var result = user.AssignRole(role);
        if (result.IsFailure) return result.Error;

        await users.UpdateAsync(user, ct);

        return new UserResponseDto(
            user.Id, user.FullName, user.Email, user.Phone.Value,
            user.Status.ToString(), user.TwoFactorEnabled, user.LastLoginAtUtc,
            user.Roles.Select(r => r.Role.Name).ToArray());
    }
}
