using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Domain;
using Jacana.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Notifications.Infrastructure.Repositories;

public sealed class NotificationMessageRepository(NotificationsDbContext db)
    : INotificationMessageRepository
{
    public Task AddAsync(NotificationMessage message, CancellationToken ct = default)
    {
        db.NotificationMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(NotificationMessage message, CancellationToken ct = default)
    {
        db.NotificationMessages.Update(message);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<NotificationMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default)
        => await db.NotificationMessages
            .Where(m => m.Status == NotificationStatus.Pending)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);
}
