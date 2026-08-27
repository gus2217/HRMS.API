using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Jacana.PatientRegistration.Infrastructure.Persistence;

/// <summary>
/// Patient Registration module DbContext, mapped to the <c>patient</c> schema.
/// Owns only this module's entities plus the shared outbox + audit tables.
/// </summary>
public sealed class PatientDbContext : DbContext
{
    private readonly IValueEncryptor _encryptor;

    public PatientDbContext(DbContextOptions<PatientDbContext> options, IValueEncryptor encryptor)
        : base(options) => _encryptor = encryptor;

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("patient");
        builder.ApplyConfigurationsFromAssembly(typeof(PatientDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();

        // NationalId — encrypted at rest via AES-GCM (key from secrets store).
        // Decrypted only into memory by EF when the owned value is materialized.
        var nationalIdConverter = new ValueConverter<string, string>(
            v => _encryptor.Encrypt(v),
            v => _encryptor.Decrypt(v));

        builder.Entity<Patient>().OwnsOne(p => p.NationalId, n =>
        {
            n.Property(x => x.Value)
                .HasColumnName("NationalId")
                .HasMaxLength(512)
                .HasConversion(nationalIdConverter);
        });

        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
