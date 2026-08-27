using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Domain;
using Jacana.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Identity.Infrastructure.Repositories;

public sealed class UserRepository(IdentityDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users
            .Include(u => u.Roles).ThenInclude(r => r.Role).ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await db.Users
            .Include(u => u.Roles).ThenInclude(r => r.Role).ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Entry(user).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default)
    {
        var codes = await db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(ur => ur.Role.Permissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(ct);
        return codes;
    }
}

public sealed class RoleRepository(IdentityDbContext db) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => db.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
        => await db.Roles.Include(r => r.Permissions).ToListAsync(ct);

    public Task AddAsync(Role role, CancellationToken ct = default)
    {
        db.Roles.Add(role);
        return Task.CompletedTask;
    }
}

public sealed class PermissionRepository(IdentityDbContext db) : IPermissionRepository
{
    public Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default)
        => db.Permissions.FirstOrDefaultAsync(p => p.Code == code, ct);

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default)
        => await db.Permissions.ToListAsync(ct);

    public Task AddAsync(Permission permission, CancellationToken ct = default)
    {
        db.Permissions.Add(permission);
        return Task.CompletedTask;
    }
}

public sealed class RefreshTokenRepository(IdentityDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        db.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        db.Entry(refreshToken).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}
