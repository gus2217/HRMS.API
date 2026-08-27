using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Domain;

public sealed class Role : AggregateRoot<Guid>
{
    private readonly List<RolePermission> _permissions = new();

    private Role() { } // EF

    private Role(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; } = string.Empty;
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public static Result<Role> Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Role name is required.");
        return new Role(id, name.Trim());
    }

    public Result Grant(Permission permission)
    {
        if (_permissions.Any(p => p.PermissionId == permission.Id)) return Result.Success();
        _permissions.Add(new RolePermission { RoleId = Id, PermissionId = permission.Id, Permission = permission });
        return Result.Success();
    }
}

public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
