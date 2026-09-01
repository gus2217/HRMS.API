namespace Jacana.Notifications.Application.DTOs;

/// <summary>An in-app notification as shown in the user's bell feed.</summary>
public sealed record UserNotificationDto(
    Guid Id,
    string Category,
    string Title,
    string Message,
    string EntityType,
    Guid? EntityId,
    bool IsRead,
    DateTime CreatedAtUtc);

public sealed record UnreadNotificationCountDto(int UnreadCount);
