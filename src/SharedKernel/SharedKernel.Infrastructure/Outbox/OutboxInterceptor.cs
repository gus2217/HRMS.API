using System.Text.Json;
using Jacana.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Jacana.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Intercepts SaveChanges, extracts domain events raised by tracked aggregates, and
/// writes them to the outbox table within the same transaction. Event delivery to
/// external systems happens later via the Hangfire dispatcher — never inline.
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        WriteOutbox(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        WriteOutbox(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void WriteOutbox(DbContext? context)
    {
        if (context is null) return;

        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var entry in aggregates)
        {
            var aggregate = entry.Entity;
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().AssemblyQualifiedName
                        ?? domainEvent.GetType().FullName!,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions),
                    OccurredAtUtc = domainEvent.OccurredAtUtc,
                    AttemptCount = 0
                });
            }
            aggregate.ClearDomainEvents();
        }
    }
}
