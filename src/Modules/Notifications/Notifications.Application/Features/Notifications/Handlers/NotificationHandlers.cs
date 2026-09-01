using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Application.DTOs;
using Jacana.Notifications.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Notifications.Application.Features.Notifications.Handlers;

public sealed class GetMyNotificationsQueryHandler(
    IUserNotificationRepository notifications,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyNotificationsQuery, Result<PagedResult<UserNotificationDto>>>
{
    public async Task<Result<PagedResult<UserNotificationDto>>> Handle(
        GetMyNotificationsQuery request, CancellationToken ct)
    {
        var items = await notifications.GetByRecipientAsync(
            currentUser.UserId, request.UnreadOnly, request.PageNumber, request.PageSize, ct);
        var total = await notifications.CountAsync(currentUser.UserId, request.UnreadOnly, ct);
        return new PagedResult<UserNotificationDto>(
            items, total, request.PageNumber, request.PageSize);
    }
}

public sealed class GetUnreadNotificationCountQueryHandler(
    IUserNotificationRepository notifications,
    ICurrentUser currentUser)
    : IRequestHandler<GetUnreadNotificationCountQuery, Result<UnreadNotificationCountDto>>
{
    public async Task<Result<UnreadNotificationCountDto>> Handle(
        GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var count = await notifications.CountAsync(currentUser.UserId, unreadOnly: true, ct);
        return new UnreadNotificationCountDto(count);
    }
}

public sealed class MarkNotificationReadCommandHandler(
    IUserNotificationRepository notifications,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<MarkNotificationReadCommand, Result<UserNotificationDto>>
{
    public async Task<Result<UserNotificationDto>> Handle(
        MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await notifications.GetByIdAsync(request.NotificationId, ct);
        if (notification is null) return Error.NotFound("Notification not found.");
        if (notification.RecipientUserId != currentUser.UserId)
            return Error.Forbidden("Notification does not belong to the current user.");

        var result = notification.MarkRead(clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await notifications.UpdateAsync(notification, ct);
        return new UserNotificationDto(
            notification.Id, notification.Category.ToString(), notification.Title,
            notification.Message, notification.EntityType, notification.EntityId,
            notification.IsRead, notification.CreatedAtUtc);
    }
}

public sealed class MarkAllNotificationsReadCommandHandler(
    IUserNotificationRepository notifications,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result<UnreadNotificationCountDto>>
{
    public async Task<Result<UnreadNotificationCountDto>> Handle(
        MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var count = await notifications.MarkAllReadAsync(currentUser.UserId, clock.UtcNow, ct);
        return new UnreadNotificationCountDto(count);
    }
}

// ── Preferences ────────────────────────────────────────────────────────────────

public sealed class GetMyNotificationPreferencesQueryHandler(
    INotificationPreferenceRepository preferences,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyNotificationPreferencesQuery, Result<IReadOnlyList<NotificationPreferenceDto>>>
{
    public async Task<Result<IReadOnlyList<NotificationPreferenceDto>>> Handle(
        GetMyNotificationPreferencesQuery request, CancellationToken ct)
    {
        var stored = await preferences.GetByUserAsync(currentUser.UserId, ct);
        var byCategory = stored.ToDictionary(p => p.Category, p => p);

        // Return every category; absent rows mean defaults-on.
        var result = Enum.GetValues<NotificationCategory>()
            .Select(cat => byCategory.TryGetValue(cat, out var p)
                ? new NotificationPreferenceDto(cat.ToString(), p.InAppEnabled, p.SmsEnabled)
                : new NotificationPreferenceDto(cat.ToString(), InAppEnabled: true, SmsEnabled: true))
            .ToList();

        return Result.Success<IReadOnlyList<NotificationPreferenceDto>>(result);
    }
}

public sealed class UpdateNotificationPreferenceCommandHandler(
    INotificationPreferenceRepository preferences,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<UpdateNotificationPreferenceCommand, Result<NotificationPreferenceDto>>
{
    public async Task<Result<NotificationPreferenceDto>> Handle(
        UpdateNotificationPreferenceCommand request, CancellationToken ct)
    {
        var existing = await preferences.GetAsync(currentUser.UserId, request.Category, ct);
        if (existing is null)
        {
            var created = NotificationPreference.Create(
                currentUser.FacilityId, currentUser.UserId, request.Category,
                request.InAppEnabled, request.SmsEnabled, clock.UtcNow);
            if (created.IsFailure) return created.Error;
            await preferences.AddAsync(created.Value, ct);
        }
        else
        {
            var updated = existing.Update(request.InAppEnabled, request.SmsEnabled, clock.UtcNow);
            if (updated.IsFailure) return updated.Error;
            await preferences.UpdateAsync(existing, ct);
        }

        return new NotificationPreferenceDto(
            request.Category.ToString(), request.InAppEnabled, request.SmsEnabled);
    }
}
