using Jacana.Laboratory.Domain;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Laboratory.Infrastructure.Persistence;

public sealed class LaboratoryDbContext : DbContext
{
    public LaboratoryDbContext(DbContextOptions<LaboratoryDbContext> options) : base(options) { }

    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("laboratory");
        builder.ApplyConfigurationsFromAssembly(typeof(LaboratoryDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();

        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
