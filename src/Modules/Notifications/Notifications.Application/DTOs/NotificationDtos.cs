namespace Jacana.Notifications.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Channel,
    string RecipientPhoneOrEmail,
    string TemplateCode,
    string Status,
    int AttemptCount);
