using Jacana.Notifications.Application.DTOs;
using Jacana.Notifications.Domain;

namespace Jacana.Notifications.Application.Abstractions;

public interface INotificationMessageRepository
{
    Task AddAsync(NotificationMessage message, CancellationToken ct = default);
    Task UpdateAsync(NotificationMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
}
