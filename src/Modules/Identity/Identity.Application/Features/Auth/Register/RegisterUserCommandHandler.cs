using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IPasswordHasher hasher,
    ICurrentUser currentUser)
    : IRequestHandler<RegisterUserCommand, Result<UserResponseDto>>
{
    public async Task<Result<UserResponseDto>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct) is not null)
            return Error.Conflict("A user with this email already exists.");

        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure) return phoneResult.Error;

        var userResult = User.Register(
            Guid.NewGuid(),
            currentUser.FacilityId,
            request.FullName,
            request.Email,
            phoneResult.Value,
            hasher.Hash(request.Password));

        if (userResult.IsFailure) return userResult.Error;

        var user = userResult.Value;

        foreach (var roleName in request.RoleNames ?? [])
        {
            var role = await roles.GetByNameAsync(roleName, ct);
            if (role is null) return Error.NotFound($"Role '{roleName}' does not exist.");
            user.AssignRole(role);
        }

        await users.AddAsync(user, ct);

        return new UserResponseDto(
            user.Id, user.FullName, user.Email, user.Phone.Value,
            user.Status.ToString(), user.TwoFactorEnabled, user.LastLoginAtUtc,
            user.Roles.Select(r => r.Role.Name).ToArray());
    }
}
