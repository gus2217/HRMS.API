using Jacana.Notifications.Application.DTOs;

namespace Jacana.Notifications.Application.Abstractions;

/// <summary>
/// Pushes a committed notification to an online recipient. The implementation
/// lives in the API host (SignalR hub context) so the Notifications module never
/// depends on ASP.NET Core transport concerns.
/// </summary>
public interface INotificationPusher
{
    Task PushAsync(Guid recipientUserId, UserNotificationDto dto, CancellationToken ct = default);
}
