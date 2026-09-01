using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Jacana.HRMS.Api.Hubs;

/// <summary>
/// Real-time notification delivery hub. The SPA connects with the access token
/// (query string for WebSockets), and the server pushes "notificationReceived"
/// to the recipient's user connection the moment a fan-out handler commits.
/// </summary>
[Authorize]
public sealed class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Group per user id so the pusher can target Clients.Group(userId) —
        // robust regardless of which claim maps to the user id.
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }
}

/// <summary>SignalR-backed implementation of the notification pusher.</summary>
public sealed class SignalRNotificationPusher(IHubContext<NotificationsHub> hub) : INotificationPusher
{
    public async Task PushAsync(Guid recipientUserId, UserNotificationDto dto, CancellationToken ct = default)
    {
        await hub.Clients.Group(recipientUserId.ToString()).SendAsync("notificationReceived", dto, ct);
    }
}
