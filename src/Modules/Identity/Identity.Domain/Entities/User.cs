using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Domain;

/// <summary>
/// A user of the facility. Password is Argon2id-hashed; the raw secret never leaves
/// the hashing boundary. Roles are assigned through Role, permissions resolved via
/// Role → RolePermission → Permission.
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    private readonly List<UserRole> _roles = new();
    private readonly List<UserPermission> _directPermissions = new();

    private User() { } // EF

    private User(
        Guid id,
        FacilityId facilityId,
        string fullName,
        string email,
        PhoneNumber phone,
        string passwordHash,
        bool mustChangePassword)
        : base(id)
    {
        FacilityId = facilityId;
        FullName = fullName;
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        MustChangePassword = mustChangePassword;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public PhoneNumber Phone { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool TwoFactorEnabled { get; private set; }
    public string? TotpSecret { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    /// <summary>Roles bundle permissions; direct grants fine-tune access per user.</summary>
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    /// <summary>Permissions granted directly to this user (beyond what their roles carry).</summary>
    public IReadOnlyCollection<UserPermission> DirectPermissions => _directPermissions.AsReadOnly();

    /// <summary>
    /// True when the next successful login must be followed by a password change
    /// (admin-created accounts and admin password resets). The auth service refuses
    /// to issue tokens until the password has been changed.
    /// </summary>
    public bool MustChangePassword { get; private set; }

    public static Result<User> Register(
        Guid id,
        FacilityId facilityId,
        string fullName,
        string email,
        PhoneNumber phone,
        string passwordHash,
        bool mustChangePassword = false)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Error.Validation("Full name is required.");
        if (string.IsNullOrWhiteSpace(email))
            return Error.Validation("Email is required.");

        return new User(id, facilityId, fullName.Trim(), email.Trim().ToLowerInvariant(), phone, passwordHash, mustChangePassword);
    }

    public Result AssignRole(Role role)
    {
        if (_roles.Any(r => r.RoleId == role.Id))
            return Result.Success(); // idempotent

        _roles.Add(new UserRole { UserId = Id, RoleId = role.Id, Role = role });
        return Result.Success();
    }

    public Result RemoveRole(Guid roleId)
    {
        var existing = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (existing is null) return Error.NotFound("Role is not assigned to this user.");
        _roles.Remove(existing);
        return Result.Success();
    }

    public void RecordLogin(DateTime atUtc) => LastLoginAtUtc = atUtc;

    public Result SetPasswordHash(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash)) return Error.Validation("Password hash is required.");
        PasswordHash = newHash;
        return Result.Success();
    }

    /// <summary>Admin reset / account creation: force a password change at the next login.</summary>
    public void RequirePasswordChange() => MustChangePassword = true;

    /// <summary>Clears the forced-change flag after the user sets their own password.</summary>
    public void MarkPasswordChanged() => MustChangePassword = false;

    public Result GrantPermission(Permission permission)
    {
        if (_directPermissions.Any(p => p.PermissionId == permission.Id))
            return Result.Success(); // idempotent

        _directPermissions.Add(new UserPermission { UserId = Id, PermissionId = permission.Id, Permission = permission });
        return Result.Success();
    }

    public Result RevokePermission(Guid permissionId)
    {
        var existing = _directPermissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (existing is null) return Error.NotFound("Permission is not granted to this user.");
        _directPermissions.Remove(existing);
        return Result.Success();
    }

    public Result EnableTwoFactor(string totpSecret)
    {
        if (string.IsNullOrWhiteSpace(totpSecret)) return Error.Validation("TOTP secret is required.");
        TotpSecret = totpSecret;
        TwoFactorEnabled = true;
        return Result.Success();
    }

    public Result Suspend() { Status = UserStatus.Suspended; return Result.Success(); }
    public Result Reactivate() { Status = UserStatus.Active; return Result.Success(); }
}

public sealed class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
