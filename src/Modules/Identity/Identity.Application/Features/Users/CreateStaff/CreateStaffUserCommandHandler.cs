using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users.CreateStaff;

/// <summary>
/// Admin creates a staff account. The password is a generated one-time default —
/// returned to the caller once so it can be handed to the staff member, then the
/// account is forced to change it at first login. Roles and optional direct
/// permission grants are applied immediately.
/// </summary>
public sealed class CreateStaffUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IPermissionRepository permissions,
    IPasswordHasher hasher,
    IDefaultPasswordGenerator passwordGenerator,
    ICurrentUser currentUser)
    : IRequestHandler<CreateStaffUserCommand, Result<CreatedStaffAccountDto>>
{
    public async Task<Result<CreatedStaffAccountDto>> Handle(CreateStaffUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(email, ct) is not null)
            return Error.Conflict("A user with this email already exists.");

        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure) return phoneResult.Error;

        var temporaryPassword = passwordGenerator.Generate();
        var userResult = User.Register(
            Guid.NewGuid(),
            currentUser.FacilityId,
            request.FullName,
            email,
            phoneResult.Value,
            hasher.Hash(temporaryPassword),
            mustChangePassword: true);
        if (userResult.IsFailure) return userResult.Error;

        var user = userResult.Value;

        foreach (var roleName in request.RoleNames)
        {
            var role = await roles.GetByNameAsync(roleName, ct);
            if (role is null) return Error.NotFound($"Role '{roleName}' does not exist.");
            user.AssignRole(role);
        }

        foreach (var code in request.PermissionCodes)
        {
            var permission = await permissions.GetByCodeAsync(code, ct);
            if (permission is null) return Error.NotFound($"Permission '{code}' does not exist.");
            user.GrantPermission(permission);
        }

        await users.AddAsync(user, ct);

        var detail = StaffUserMapping.ToDetail(user);
        return new CreatedStaffAccountDto(detail, temporaryPassword);
    }
}
