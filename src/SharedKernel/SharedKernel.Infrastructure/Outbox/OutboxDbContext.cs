using Microsoft.EntityFrameworkCore;

namespace Jacana.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Minimal DbContext owning only the shared outbox table. Used by the Hangfire
/// dispatcher to read/publish pending messages written by any module's DbContext
/// (all modules map <see cref="OutboxMessage"/> to the same shared table).
/// </summary>
public sealed class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<OutboxMessage>().ToTable("outbox_messages", "outbox");
        builder.Entity<OutboxMessage>().HasKey(m => m.Id);
        builder.Entity<OutboxMessage>().Property(m => m.Type).IsRequired();
        builder.Entity<OutboxMessage>().Property(m => m.Payload).IsRequired();
        builder.Entity<OutboxMessage>().Property(m => m.OccurredAtUtc).IsRequired();
    }
}
