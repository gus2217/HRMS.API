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
}
