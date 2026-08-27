using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.SharedKernel.Infrastructure;

/// <summary>
/// Maps the shared outbox + audit tables into a module's model WITHOUT generating
/// migrations for them. The owning contexts (OutboxDbContext, AuditDbContext) own the
/// DDL; module contexts only need the entity mapped so the interceptors can write to
/// them in the same transaction.
/// </summary>
public static class SharedTableMappings
{
    public static void MapSharedTables(this ModelBuilder builder)
    {
        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);

        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
