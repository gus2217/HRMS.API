using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.PatientClinical.Handlers;

// ── Patient flags ──────────────────────────────────────────────────────────────

public sealed class RaisePatientFlagCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<RaisePatientFlagCommand, Result<PatientFlagDto>>
{
    public async Task<Result<PatientFlagDto>> Handle(RaisePatientFlagCommand request, CancellationToken ct)
    {
        var flag = PatientFlag.Raise(
            currentUser.FacilityId, request.PatientId, request.Type, request.Message,
            currentUser.UserId, clock.UtcNow);
        if (flag.IsFailure) return flag.Error;

        await repository.AddPatientFlagAsync(flag.Value, ct);
        return MapFlag(flag.Value);
    }

    internal static PatientFlagDto MapFlag(PatientFlag f) => new(
        f.Id, f.PatientId, f.Type.ToString(), f.Message, f.IsActive,
        f.CreatedByUserId, f.CreatedAtUtc, f.DeactivatedByUserId, f.DeactivatedAtUtc);
}

public sealed class DeactivatePatientFlagCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<DeactivatePatientFlagCommand, Result<PatientFlagDto>>
{
    public async Task<Result<PatientFlagDto>> Handle(DeactivatePatientFlagCommand request, CancellationToken ct)
    {
        var flag = await repository.GetPatientFlagAsync(request.FlagId, ct);
        if (flag is null) return Error.NotFound("Flag not found.");

        var result = flag.Deactivate(currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        return RaisePatientFlagCommandHandler.MapFlag(flag);
    }
}

// ── Attachments ────────────────────────────────────────────────────────────────

public sealed class UploadAttachmentCommandHandler(
    IPatientClinicalRepository repository,
    IFileStorage fileStorage,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<UploadAttachmentCommand, Result<PatientAttachmentDto>>
{
    public async Task<Result<PatientAttachmentDto>> Handle(UploadAttachmentCommand request, CancellationToken ct)
    {
        var attachmentId = Guid.NewGuid();
        // Storage key is patient-scoped and collision-proof (id + sanitised name).
        var storageKey = $"{request.PatientId:N}/{attachmentId:N}/{Sanitize(request.FileName)}";

        var attachment = PatientAttachment.Create(
            currentUser.FacilityId, request.PatientId, request.FileName, request.ContentType,
            request.Content.Length, request.Category, storageKey, currentUser.UserId, clock.UtcNow);
        if (attachment.IsFailure) return attachment.Error;

        // Persist bytes BEFORE the aggregate so a failed save can't leave a dangling DB row.
        await fileStorage.SaveAsync(storageKey, request.Content, ct);
        await repository.AddAttachmentAsync(attachment.Value, ct);

        var a = attachment.Value;
        return new PatientAttachmentDto(
            a.Id, a.PatientId, a.FileName, a.ContentType, a.SizeBytes, a.Category,
            a.UploadedByUserId, a.UploadedAtUtc);
    }

    internal static string Sanitize(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(fileName.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "file" : safe;
    }
}

public sealed class DeleteAttachmentCommandHandler(
    IPatientClinicalRepository repository,
    IFileStorage fileStorage)
    : IRequestHandler<DeleteAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteAttachmentCommand request, CancellationToken ct)
    {
        var attachment = await repository.GetAttachmentAsync(request.AttachmentId, ct);
        if (attachment is null) return Error.NotFound("Attachment not found.");

        await fileStorage.DeleteAsync(attachment.StorageKey, ct);
        await repository.DeleteAttachmentAsync(attachment, ct);
        return Result.Success();
    }
}

// ── Diagnostic orders ──────────────────────────────────────────────────────────

public sealed class CreateDiagnosticOrderCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CreateDiagnosticOrderCommand, Result<DiagnosticOrderDto>>
{
    public async Task<Result<DiagnosticOrderDto>> Handle(CreateDiagnosticOrderCommand request, CancellationToken ct)
    {
        var order = DiagnosticOrder.Create(
            currentUser.FacilityId, request.PatientId, request.ConsultationId,
            request.Type, request.Name, request.BodySite, request.ClinicalIndication,
            request.Priority, currentUser.UserId, clock.UtcNow);
        if (order.IsFailure) return order.Error;

        await repository.AddDiagnosticOrderAsync(order.Value, ct);
        return MapOrder(order.Value);
    }

    internal static DiagnosticOrderDto MapOrder(DiagnosticOrder o) => new(
        o.Id, o.PatientId, o.ConsultationId, o.Type.ToString(), o.Name, o.BodySite,
        o.ClinicalIndication, o.Priority.ToString(), o.Status.ToString(),
        o.OrderedByUserId, o.OrderedAtUtc, o.ScheduledByUserId, o.ScheduledAtUtc,
        o.PerformedByUserId, o.PerformedAtUtc, o.Report, o.ReportedByUserId, o.ReportedAtUtc,
        o.CancelledByUserId, o.CancelledAtUtc, o.CancellationReason);
}

public sealed class ScheduleDiagnosticOrderCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<ScheduleDiagnosticOrderCommand, Result<DiagnosticOrderDto>>
{
    public async Task<Result<DiagnosticOrderDto>> Handle(ScheduleDiagnosticOrderCommand request, CancellationToken ct)
    {
        var order = await repository.GetDiagnosticOrderAsync(request.OrderId, ct);
        if (order is null) return Error.NotFound("Diagnostic order not found.");

        var result = order.Schedule(currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await repository.UpdateDiagnosticOrderAsync(order, ct);
        return CreateDiagnosticOrderCommandHandler.MapOrder(order);
    }
}

public sealed class PerformDiagnosticOrderCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<PerformDiagnosticOrderCommand, Result<DiagnosticOrderDto>>
{
    public async Task<Result<DiagnosticOrderDto>> Handle(PerformDiagnosticOrderCommand request, CancellationToken ct)
    {
        var order = await repository.GetDiagnosticOrderAsync(request.OrderId, ct);
        if (order is null) return Error.NotFound("Diagnostic order not found.");

        var result = order.MarkPerformed(currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await repository.UpdateDiagnosticOrderAsync(order, ct);
        return CreateDiagnosticOrderCommandHandler.MapOrder(order);
    }
}

public sealed class ReportDiagnosticOrderCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<ReportDiagnosticOrderCommand, Result<DiagnosticOrderDto>>
{
    public async Task<Result<DiagnosticOrderDto>> Handle(ReportDiagnosticOrderCommand request, CancellationToken ct)
    {
        var order = await repository.GetDiagnosticOrderAsync(request.OrderId, ct);
        if (order is null) return Error.NotFound("Diagnostic order not found.");

        var result = order.RecordReport(request.Report, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await repository.UpdateDiagnosticOrderAsync(order, ct);
        return CreateDiagnosticOrderCommandHandler.MapOrder(order);
    }
}

public sealed class CancelDiagnosticOrderCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CancelDiagnosticOrderCommand, Result<DiagnosticOrderDto>>
{
    public async Task<Result<DiagnosticOrderDto>> Handle(CancelDiagnosticOrderCommand request, CancellationToken ct)
    {
        var order = await repository.GetDiagnosticOrderAsync(request.OrderId, ct);
        if (order is null) return Error.NotFound("Diagnostic order not found.");

        var result = order.Cancel(request.Reason, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await repository.UpdateDiagnosticOrderAsync(order, ct);
        return CreateDiagnosticOrderCommandHandler.MapOrder(order);
    }
}

// ── Queries ────────────────────────────────────────────────────────────────────

public sealed class GetActiveFlagsQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetActiveFlagsQuery, Result<IReadOnlyList<PatientFlagDto>>>
{
    public async Task<Result<IReadOnlyList<PatientFlagDto>>> Handle(GetActiveFlagsQuery request, CancellationToken ct)
        => Result.Success(await repository.GetActiveFlagsAsync(request.PatientId, ct));
}

public sealed class GetAllFlagsQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetAllFlagsQuery, Result<IReadOnlyList<PatientFlagDto>>>
{
    public async Task<Result<IReadOnlyList<PatientFlagDto>>> Handle(GetAllFlagsQuery request, CancellationToken ct)
        => Result.Success(await repository.GetAllFlagsAsync(request.PatientId, ct));
}

public sealed class GetAttachmentsQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetAttachmentsQuery, Result<IReadOnlyList<PatientAttachmentDto>>>
{
    public async Task<Result<IReadOnlyList<PatientAttachmentDto>>> Handle(GetAttachmentsQuery request, CancellationToken ct)
        => Result.Success(await repository.GetAttachmentsAsync(request.PatientId, ct));
}

public sealed class GetDiagnosticOrdersByPatientQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetDiagnosticOrdersByPatientQuery, Result<IReadOnlyList<DiagnosticOrderDto>>>
{
    public async Task<Result<IReadOnlyList<DiagnosticOrderDto>>> Handle(GetDiagnosticOrdersByPatientQuery request, CancellationToken ct)
        => Result.Success(await repository.GetDiagnosticOrdersByPatientAsync(request.PatientId, ct));
}

public sealed class GetDiagnosticOrdersByConsultationQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetDiagnosticOrdersByConsultationQuery, Result<IReadOnlyList<DiagnosticOrderDto>>>
{
    public async Task<Result<IReadOnlyList<DiagnosticOrderDto>>> Handle(GetDiagnosticOrdersByConsultationQuery request, CancellationToken ct)
        => Result.Success(await repository.GetDiagnosticOrdersByConsultationAsync(request.ConsultationId, ct));
}
