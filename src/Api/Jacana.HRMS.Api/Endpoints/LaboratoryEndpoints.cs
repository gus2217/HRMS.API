using Jacana.Identity.Application;
using Jacana.Laboratory.Application.Features.Laboratory;
using Jacana.Laboratory.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Laboratory endpoints: bind → dispatch → map result → return.</summary>
public static class LaboratoryEndpoints
{
    public static IEndpointRouteBuilder MapLaboratoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/lab");

        group.MapPost("/orders", CreateLabOrderAsync)
            .RequireAuthorization(Permissions.Lab.Order);

        group.MapGet("/orders", SearchLabOrdersAsync)
            .RequireAuthorization(Permissions.Lab.Order);

        group.MapGet("/orders/{id:guid}", GetLabOrderAsync)
            .RequireAuthorization(Permissions.Lab.Order);

        group.MapPost("/orders/{id:guid}/results", RecordResultAsync)
            .RequireAuthorization(Permissions.Lab.RecordResult);

        return app;
    }

    private static async Task<IResult> CreateLabOrderAsync(
        CreateLabOrderRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateLabOrderCommand(
            request.PatientId, request.ConsultationId,
            request.Tests.Select(t => new LabTestInput(t.TestCode, t.TestName)).ToArray()), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/lab/orders/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SearchLabOrdersAsync(
        ISender sender, CancellationToken ct, string? status = null, Guid? patientId = null, int pageNumber = 1, int pageSize = 20)
    {
        var result = await sender.Send(new SearchLabOrdersQuery(pageNumber, pageSize, status, patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetLabOrderAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetLabOrderQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RecordResultAsync(
        Guid id, RecordLabResultRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordLabResultCommand(
            id, request.TestItemId, request.ResultValue, request.ResultUnit,
            request.ReferenceRange, request.IsAbnormal), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.InvalidOperation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
