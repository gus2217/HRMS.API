using Jacana.Identity.Application;
using Jacana.Audit.Application.Features.Audit;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Audit (read-only) endpoints: bind → dispatch → map result → return.</summary>
public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/audit", GetAuditLogAsync)
            .RequireAuthorization(Permissions.Users.View);

        return app;
    }

    private static async Task<IResult> GetAuditLogAsync(
        string? entityType, int pageNumber, int pageSize, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAuditLogQuery(entityType, pageNumber, pageSize), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
