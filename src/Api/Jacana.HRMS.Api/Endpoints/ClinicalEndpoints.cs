using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Application.Features.Consultations;
using Jacana.Clinical.Domain;
using Jacana.Identity.Application;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Clinical/Consultation endpoints: bind → dispatch → map result → return.
/// No business logic lives here.
/// </summary>
public static class ClinicalEndpoints
{
    public static IEndpointRouteBuilder MapClinicalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/consultations");

        group.MapPost("/", StartAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapGet("/{id:guid}", GetAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapPost("/{id:guid}/triage", RecordTriageAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/{id:guid}/begin", BeginClinicalPhaseAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/{id:guid}/diagnoses", RecordDiagnosisAsync)
            .RequireAuthorization(Permissions.Clinical.RecordDiagnosis);

        group.MapPost("/{id:guid}/notes", AddClinicalNoteAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPut("/{id:guid}/documentation", SaveDocumentationAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/{id:guid}/referrals", CreateReferralAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/{id:guid}/complete", CompleteAsync)
            .RequireAuthorization(Permissions.Clinical.RecordDiagnosis);

        app.MapGet("/api/v1/patients/{patientId:guid}/clinical-history", GetHistoryAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        return app;
    }

    private static async Task<IResult> StartAsync(
        StartConsultationRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new StartConsultationCommand(request.PatientId, request.ClinicianUserId), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/consultations/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        ISender sender, CancellationToken ct, string? status = null, int pageNumber = 1, int pageSize = 20)
    {
        var result = await sender.Send(new SearchConsultationsQuery(pageNumber, pageSize, status), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetConsultationQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RecordTriageAsync(
        Guid id, RecordTriageRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordTriageCommand(
            id, request.TemperatureCelsius, request.BloodPressure, request.PulseRate,
            request.RespiratoryRate, request.WeightKg), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> BeginClinicalPhaseAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new BeginClinicalPhaseCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RecordDiagnosisAsync(
        Guid id, RecordDiagnosisRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordDiagnosisCommand(
            id, request.IcdCode, request.Description, request.IsPrimary), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AddClinicalNoteAsync(
        Guid id, AddClinicalNoteRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddClinicalNoteCommand(id, request.Content), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SaveDocumentationAsync(
        Guid id, SaveDocumentationRequestDto request, ISender sender, CancellationToken ct)
    {
        var data = new ClinicalDocumentationDataInput(
            request.ChiefComplaint, request.HistoryOfPresentingIllness,
            request.PastMedicalHistory, request.PastSurgicalHistory,
            request.FamilyHistory, request.SocialHistory,
            request.GynaecologicalHistory, request.ObstetricHistory, request.DrugHistory,
            request.RosGeneral, request.RosCardiovascular, request.RosRespiratory,
            request.RosGastrointestinal, request.RosGenitourinary, request.RosMusculoskeletal,
            request.RosNeurological, request.RosDermatological, request.RosEntEyes, request.RosEndocrine,
            request.ExamGeneralAppearance, request.ExamHeadAndNeck, request.ExamCardiovascular,
            request.ExamRespiratory, request.ExamAbdominal, request.ExamGenitourinary,
            request.ExamMusculoskeletal, request.ExamNeurological, request.ExamSkin, request.ExamLymphatic);

        var result = await sender.Send(new SaveDocumentationCommand(id, data), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CreateReferralAsync(
        Guid id, CreateReferralRequestDto request, ISender sender, CancellationToken ct)
    {
        if (!Enum.TryParse<ReferralPriority>(request.Priority, ignoreCase: true, out var priority))
            return Results.BadRequest(new { error = "Referral priority is invalid." });

        var result = await sender.Send(new CreateReferralCommand(
            id, request.ReferredToFacility, request.ReferredToUnit,
            request.Reason, priority, request.Notes), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CompleteAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CompleteConsultationCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetHistoryAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetPatientHistoryQuery(patientId), ct);
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
