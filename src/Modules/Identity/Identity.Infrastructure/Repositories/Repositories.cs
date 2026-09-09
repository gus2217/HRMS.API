using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Domain;
using Jacana.Identity.Infrastructure.Persistence;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Identity.Infrastructure.Repositories;

public sealed class UserRepository(IdentityDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users
            .Include(u => u.Roles).ThenInclude(r => r.Role).ThenInclude(r => r.Permissions).ThenInclude(p => p.Permission)
            .Include(u => u.DirectPermissions).ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await db.Users
            .Include(u => u.Roles).ThenInclude(r => r.Role).ThenInclude(r => r.Permissions).ThenInclude(p => p.Permission)
            .Include(u => u.DirectPermissions).ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still Detached.
        db.MarkNewChildrenAdded(user);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default)
    {
        // Effective access = role-derived permissions ∪ direct per-user grants.
        var roleCodes = db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(ur => ur.Role.Permissions)
            .Select(rp => rp.Permission.Code);

        var directCodes = db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.DirectPermissions)
            .Select(up => up.Permission.Code);

        return await roleCodes.Concat(directCodes).Distinct().ToListAsync(ct);
    }

    public async Task<PagedResult<StaffUserListItemDto>> GetPageAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = db.Users.AsNoTracking()
            .Include(u => u.Roles).ThenInclude(r => r.Role)
            .Include(u => u.DirectPermissions).ThenInclude(p => p.Permission)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Phone.Value.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = users.Select(u => new StaffUserListItemDto(
            u.Id,
            u.FullName,
            u.Email,
            u.Phone.Value,
            u.Status.ToString(),
            u.MustChangePassword,
            u.TwoFactorEnabled,
            u.LastLoginAtUtc,
            u.Roles.Select(r => r.Role.Name).OrderBy(n => n).ToArray(),
            u.DirectPermissions.Select(p => p.Permission.Code).OrderBy(c => c).ToArray())).ToArray();

        return new PagedResult<StaffUserListItemDto>(items, total, pageNumber, pageSize);
    }
}

public sealed class RoleRepository(IdentityDbContext db) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Roles.Include(r => r.Permissions).ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => db.Roles.Include(r => r.Permissions).ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
        => await db.Roles.Include(r => r.Permissions).ThenInclude(p => p.Permission).ToListAsync(ct);

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
        // Entity already tracked from GetByIdAsync; mutations auto-detected.
        // Never force graph state — it marks new children as Modified (UPDATE 0 rows).
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in active)
            token.Revoke();
    }
}
