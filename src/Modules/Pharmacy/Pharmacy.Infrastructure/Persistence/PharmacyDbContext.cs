using Jacana.Pharmacy.Domain;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Pharmacy.Infrastructure.Persistence;

public sealed class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<DispenseRecord> DispenseRecords => Set<DispenseRecord>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("pharmacy");
        builder.ApplyConfigurationsFromAssembly(typeof(PharmacyDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();

        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
