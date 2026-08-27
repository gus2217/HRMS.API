using FluentValidation;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Application.Features.Users;

namespace Jacana.Identity.Application.Features.Auth;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty();
    }
}

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
