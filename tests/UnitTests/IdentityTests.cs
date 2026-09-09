using Jacana.Identity.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Identity;

public class UserTests
{
    [Fact]
    public void Register_creates_active_user()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var result = User.Register(Guid.NewGuid(), FacilityId.New(), "Jane Doe", "jane@example.com", phone, "hash");

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, result.Value.Status);
        Assert.Empty(result.Value.Roles);
    }

    [Fact]
    public void AssignRole_is_idempotent()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var user = User.Register(Guid.NewGuid(), FacilityId.New(), "Jane", "jane@example.com", phone, "hash").Value;
        var role = Role.Create(Guid.NewGuid(), "Doctor").Value;

        user.AssignRole(role);
        user.AssignRole(role);

        Assert.Single(user.Roles);
    }

    [Fact]
    public void RefreshToken_rotation_revokes_old()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(1));
        Assert.True(token.IsActive(DateTime.UtcNow));

        token.Revoke(Guid.NewGuid());
        Assert.False(token.IsActive(DateTime.UtcNow));
    }

    [Fact]
    public void Register_with_mustChangePassword_creates_pending_account()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var user = User.Register(Guid.NewGuid(), FacilityId.New(), "Jane", "jane@x.com", phone, "hash", mustChangePassword: true).Value;

        Assert.True(user.MustChangePassword);
    }

    [Fact]
    public void GrantPermission_is_idempotent_and_revocable()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var user = User.Register(Guid.NewGuid(), FacilityId.New(), "Jane", "jane@x.com", phone, "hash").Value;
        var permission = Permission.Create(Guid.NewGuid(), "Billing.View", "View billing").Value;

        Assert.True(user.GrantPermission(permission).IsSuccess);
        Assert.True(user.GrantPermission(permission).IsSuccess); // idempotent
        Assert.Single(user.DirectPermissions);

        var revoke = user.RevokePermission(permission.Id);
        Assert.True(revoke.IsSuccess);
        Assert.Empty(user.DirectPermissions);

        // Revoking a grant the user does not hold fails cleanly.
        Assert.True(user.RevokePermission(permission.Id).IsFailure);
    }

    [Fact]
    public void PasswordChangeFlag_flips_on_require_and_mark()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var user = User.Register(Guid.NewGuid(), FacilityId.New(), "Jane", "jane@x.com", phone, "hash").Value;
        Assert.False(user.MustChangePassword);

        user.RequirePasswordChange();
        Assert.True(user.MustChangePassword);

        user.MarkPasswordChanged();
        Assert.False(user.MustChangePassword);
    }
}
