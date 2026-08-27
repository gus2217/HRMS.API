using Jacana.Identity.Application;
using Jacana.Reporting.Application.Features.Reporting;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Reporting endpoints (read-only): bind → dispatch → map result → return.</summary>
public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports");

        group.MapGet("/registrations", DailyRegistrationsAsync)
            .RequireAuthorization(Permissions.Users.View);

        group.MapGet("/revenue-by-service", RevenueByServiceAsync)
            .RequireAuthorization(Permissions.Billing.View);

        group.MapGet("/stock-levels", StockLevelsAsync)
            .RequireAuthorization(Permissions.Inventory.Receive);

        group.MapGet("/sha-claims", ShaClaimStatusAsync)
            .RequireAuthorization(Permissions.Billing.View);

        group.MapGet("/clinician-workload", ClinicianWorkloadAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapGet("/dashboard", DashboardAsync)
            .RequireAuthorization(Permissions.Users.View);

        return app;
    }

    private static async Task<IResult> DailyRegistrationsAsync(
        DateOnly from, DateOnly to, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DailyRegistrationsReportQuery(from, to), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RevenueByServiceAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RevenueByServiceReportQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> StockLevelsAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new StockLevelReportQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ShaClaimStatusAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ShaClaimStatusReportQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ClinicianWorkloadAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ClinicianWorkloadQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DashboardAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new FacilityDashboardSummaryQuery(), ct);
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
