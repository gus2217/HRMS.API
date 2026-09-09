using Jacana.HRMS.Api.Auth;
using Jacana.Identity.Application;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Application.Features.Users;
using Jacana.SharedKernel.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Staff-management endpoints: directory, account creation with a one-time
/// default password, role + permission assignment, suspend/reactivate and
/// admin password resets. Every route is gated by a permission policy — never
/// a hardcoded role.
/// </summary>
public static class StaffEndpoints
{
    public static IEndpointRouteBuilder MapStaffEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity");

        // Directory + lookups
        group.MapGet("/users", ListUsersAsync)
            .RequireAuthorization(Permissions.Users.View);
        group.MapGet("/users/{id:guid}", GetUserAsync)
            .RequireAuthorization(Permissions.Users.View);
        group.MapGet("/roles", ListRolesAsync)
            .RequireAuthorization(Permissions.Roles.View);
        group.MapGet("/permissions", ListPermissionsAsync)
            .RequireAuthorization(Permissions.Roles.View);

        // Account lifecycle
        group.MapPost("/users", CreateStaffAsync)
            .RequireAuthorization(Permissions.Users.Register);
        group.MapPut("/users/{id:guid}/access", UpdateAccessAsync)
            .RequireAuthorization(Permissions.Users.AssignRole, Permissions.Users.ManagePermissions);
        group.MapPost("/users/{id:guid}/suspend", SuspendAsync)
            .RequireAuthorization(Permissions.Users.Suspend);
        group.MapPost("/users/{id:guid}/reactivate", ReactivateAsync)
            .RequireAuthorization(Permissions.Users.Suspend);
        group.MapPost("/users/{id:guid}/reset-password", ResetPasswordAsync)
            .RequireAuthorization(Permissions.Users.ResetPassword);

        return app;
    }

    private static async Task<IResult> ListUsersAsync(
        int pageNumber, int pageSize, string? search, ISender sender, CancellationToken ct)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;

        var result = await sender.Send(new GetUsersQuery(pageNumber, pageSize, search), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetUserAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserDetailQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CreateStaffAsync(
        CreateStaffRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateStaffUserCommand(
            request.FullName,
            request.Email,
            request.Phone,
            request.RoleNames ?? [],
            request.PermissionCodes ?? []), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/identity/users/{result.Value.User.Id}", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> UpdateAccessAsync(
        Guid id, UpdateUserAccessRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateUserAccessCommand(
            id, request.RoleNames ?? [], request.PermissionCodes ?? []), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SuspendAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SetUserStatusCommand(id, Suspend: true), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ReactivateAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SetUserStatusCommand(id, Suspend: false), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ResetPasswordAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ResetUserPasswordCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ListRolesAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListRolesQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ListPermissionsAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListPermissionsQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.Conflict => Results.Conflict(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
