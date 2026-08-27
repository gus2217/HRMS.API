using System.Text.Json;
using Jacana.SharedKernel.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jacana.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Pulls pending outbox messages and publishes them as MediatR notifications.
/// Delivery is retried by Hangfire on failure. Run via a recurring background job.
/// </summary>
public sealed class OutboxDispatcher(
    OutboxDbContext dbContext,
    IPublisher publisher,
    ILogger<OutboxDispatcher> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task DispatchAsync(int batchSize = 20, CancellationToken ct = default)
    {
        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAtUtc == null && m.AttemptCount < 5)
            .OrderBy(m => m.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    logger.LogWarning("Outbox message {Id} references unknown type {Type}", message.Id, message.Type);
                    message.LastError = $"Unknown type {message.Type}";
                    message.ProcessedAtUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, SerializerOptions)
                    ?? throw new InvalidOperationException($"Could not deserialize {message.Type}.");

                // Wrap the BCL-only domain event in a MediatR notification and publish.
                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
                var notification = Activator.CreateInstance(notificationType, domainEvent)!;
                await publisher.Publish((INotification)notification, ct);

                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount++;
                message.LastError = ex.Message;
                logger.LogError(ex, "Failed to dispatch outbox message {Id} (attempt {Attempt})",
                    message.Id, message.AttemptCount);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
