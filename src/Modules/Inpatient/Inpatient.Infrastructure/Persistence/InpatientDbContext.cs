using Jacana.Inpatient.Domain;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inpatient.Infrastructure.Persistence;

public sealed class InpatientDbContext : DbContext
{
    public InpatientDbContext(DbContextOptions<InpatientDbContext> options) : base(options) { }

    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<WardMedicalRecord> WardMedicalRecords => Set<WardMedicalRecord>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("inpatient");
        builder.ApplyConfigurationsFromAssembly(typeof(InpatientDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();

        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
