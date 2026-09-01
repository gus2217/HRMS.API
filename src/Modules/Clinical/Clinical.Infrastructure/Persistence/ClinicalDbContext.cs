using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Clinical.Infrastructure.Persistence;

/// <summary>
/// Clinical module DbContext, mapped to the <c>clinical</c> schema. Owns only this
/// module's entities plus the shared outbox + audit tables.
/// </summary>
public sealed class ClinicalDbContext : DbContext
{
    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options) { }

    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Immunization> Immunizations => Set<Immunization>();
    public DbSet<Condition> Conditions => Set<Condition>();
    public DbSet<ClinicalDocumentation> ClinicalDocumentations => Set<ClinicalDocumentation>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentRequest> AppointmentRequests => Set<AppointmentRequest>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("clinical");
        builder.ApplyConfigurationsFromAssembly(typeof(ClinicalDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();

        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
