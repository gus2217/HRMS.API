using Jacana.Notifications.Application.DTOs;
using Jacana.Notifications.Domain;

namespace Jacana.Notifications.Application.Abstractions;

/// <summary>Read/write side for in-app user notifications.</summary>
public interface IUserNotificationRepository
{
    Task AddAsync(UserNotification notification, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<UserNotification> notifications, CancellationToken ct = default);
    Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(UserNotification notification, CancellationToken ct = default);

    Task<IReadOnlyList<UserNotificationDto>> GetByRecipientAsync(
        Guid recipientUserId, bool unreadOnly, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(Guid recipientUserId, bool unreadOnly, CancellationToken ct = default);
    Task<int> MarkAllReadAsync(Guid recipientUserId, DateTime readAtUtc, CancellationToken ct = default);
}
