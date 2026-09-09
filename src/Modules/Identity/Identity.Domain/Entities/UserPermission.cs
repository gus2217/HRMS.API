namespace Jacana.Identity.Domain;

/// <summary>
/// A permission granted directly to a user (not via a role). Lets an administrator
/// fine-tune access beyond the user's role bundles — e.g. grant Billing.View to a
/// Nurse without giving them the Accountant role. Effective permissions are the
/// union of role-derived and direct grants.
/// </summary>
public sealed class UserPermission
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
