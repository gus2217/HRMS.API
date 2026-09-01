namespace Jacana.Notifications.Application.DTOs;

/// <summary>HTTP request binding for updating one notification preference.</summary>
public sealed record UpdateNotificationPreferenceRequestDto(
    string Category,
    bool InAppEnabled,
    bool SmsEnabled);
