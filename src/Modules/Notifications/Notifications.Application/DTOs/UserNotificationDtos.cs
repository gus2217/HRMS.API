namespace Jacana.Notifications.Application.DTOs;

/// <summary>An in-app notification as shown in the user's bell feed.</summary>
public sealed record UserNotificationDto(
    Guid Id,
    string Category,
    string Title,
    string Message,
    string EntityType,
    Guid? EntityId,
    string? Link,
    bool IsRead,
    DateTime CreatedAtUtc);

public sealed record UnreadNotificationCountDto(int UnreadCount);

/// <summary>Per-user, per-category delivery preference (defaults-on).</summary>
public sealed record NotificationPreferenceDto(
    string Category,
    bool InAppEnabled,
    bool SmsEnabled);
