using Jacana.Identity.Domain;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Identity.Infrastructure.Persistence;

/// <summary>
/// Identity module DbContext, mapped to the <c>identity</c> schema. Owns only the
/// Identity module's entities plus the shared outbox + audit tables.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identity");
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        // Shared outbox + audit live in their own schema.
        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
