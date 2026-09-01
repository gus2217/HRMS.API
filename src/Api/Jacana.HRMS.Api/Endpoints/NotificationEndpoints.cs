using Jacana.Notifications.Application.Features.Notifications;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// In-app notification endpoints (bell feed). Any authenticated user reads their
/// own notifications — no role permission required, ownership is enforced in the
/// handlers via the current user id.
/// </summary>
public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications");

        group.MapGet("/", GetMyAsync).RequireAuthorization();
        group.MapGet("/unread-count", GetUnreadCountAsync).RequireAuthorization();
        group.MapPost("/{id:guid}/read", MarkReadAsync).RequireAuthorization();
        group.MapPost("/read-all", MarkAllReadAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetMyAsync(
        ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20, bool unreadOnly = false)
    {
        var result = await sender.Send(new GetMyNotificationsQuery(pageNumber, pageSize, unreadOnly), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetUnreadCountAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetUnreadNotificationCountQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> MarkReadAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new MarkNotificationReadCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> MarkAllReadAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new MarkAllNotificationsReadCommand(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.InvalidOperation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Conflict => Results.Conflict(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
