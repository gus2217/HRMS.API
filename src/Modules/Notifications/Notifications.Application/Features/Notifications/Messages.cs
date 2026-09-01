using Jacana.Notifications.Application.DTOs;
using Jacana.Notifications.Domain;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Notifications.Application.Features.Notifications;

public sealed record GetMyNotificationsQuery(int PageNumber, int PageSize, bool UnreadOnly = false)
    : IQuery<Result<PagedResult<UserNotificationDto>>>;

public sealed record GetUnreadNotificationCountQuery()
    : IQuery<Result<UnreadNotificationCountDto>>;

public sealed record MarkNotificationReadCommand(Guid NotificationId)
    : ICommand<Result<UserNotificationDto>>;

public sealed record MarkAllNotificationsReadCommand()
    : ICommand<Result<UnreadNotificationCountDto>>;

// ── Preferences ────────────────────────────────────────────────────────────────

public sealed record GetMyNotificationPreferencesQuery()
    : IQuery<Result<IReadOnlyList<NotificationPreferenceDto>>>;

public sealed record UpdateNotificationPreferenceCommand(
    NotificationCategory Category, bool InAppEnabled, bool SmsEnabled)
    : ICommand<Result<NotificationPreferenceDto>>;
