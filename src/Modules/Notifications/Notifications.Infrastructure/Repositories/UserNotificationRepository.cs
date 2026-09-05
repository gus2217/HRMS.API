using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Application.DTOs;
using Jacana.Notifications.Domain;
using Jacana.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Notifications.Infrastructure.Repositories;

public sealed class UserNotificationRepository(NotificationsDbContext db) : IUserNotificationRepository
{
    public Task AddAsync(UserNotification notification, CancellationToken ct = default)
    {
        db.UserNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<UserNotification> notifications, CancellationToken ct = default)
    {
        db.UserNotifications.AddRange(notifications);
        return Task.CompletedTask;
    }

    public Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.UserNotifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public Task UpdateAsync(UserNotification notification, CancellationToken ct = default)
        => Task.CompletedTask; // already tracked from GetByIdAsync

    public async Task<IReadOnlyList<UserNotificationDto>> GetByRecipientAsync(
        Guid recipientUserId, bool unreadOnly, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.UserNotifications.AsNoTracking()
            .Where(n => n.RecipientUserId == recipientUserId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => Map(n))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(Guid recipientUserId, bool unreadOnly, CancellationToken ct = default)
    {
        var query = db.UserNotifications.AsNoTracking()
            .Where(n => n.RecipientUserId == recipientUserId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        return query.CountAsync(ct);
    }

    public async Task<int> MarkAllReadAsync(Guid recipientUserId, DateTime readAtUtc, CancellationToken ct = default)
        => await db.UserNotifications
            .Where(n => n.RecipientUserId == recipientUserId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAtUtc, readAtUtc), ct);

    private static UserNotificationDto Map(UserNotification n) => new(
        n.Id, n.Category.ToString(), n.Title, n.Message, n.EntityType, n.EntityId,
        n.Link, n.IsRead, n.CreatedAtUtc);
}
