using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Application.DTOs;
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
