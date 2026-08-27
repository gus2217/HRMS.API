using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Audit.Infrastructure.Persistence;

/// <summary>
/// Read-only DbContext for the audit trail. Maps the shared AuditLogEntry to the
/// <c>audit</c> schema (written by the AuditingSaveChangesInterceptor in other modules).
/// </summary>
public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit");
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
