using Jacana.Inventory.Domain;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("inventory");
        builder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();

        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox", t => t.ExcludeFromMigrations());
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<AuditLogEntry>().ToTable("audit_log", "audit", t => t.ExcludeFromMigrations());
        builder.Entity<AuditLogEntry>().HasKey(a => a.Id);
    }
}
