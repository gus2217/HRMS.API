using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.PatientClinical;

public sealed record RaisePatientFlagCommand(
    Guid PatientId,
    PatientFlagType Type,
    string Message)
    : ICommand<Result<PatientFlagDto>>;

public sealed record DeactivatePatientFlagCommand(
    Guid FlagId)
    : ICommand<Result<PatientFlagDto>>;

public sealed record UploadAttachmentCommand(
    Guid PatientId,
    string FileName,
    string ContentType,
    string Category,
    byte[] Content)
    : ICommand<Result<PatientAttachmentDto>>;

public sealed record DeleteAttachmentCommand(
    Guid AttachmentId)
    : ICommand<Result>;

public sealed record CreateDiagnosticOrderCommand(
    Guid PatientId,
    Guid? ConsultationId,
    DiagnosticOrderType Type,
    string Name,
    string? BodySite,
    string ClinicalIndication,
    DiagnosticOrderPriority Priority)
    : ICommand<Result<DiagnosticOrderDto>>;

public sealed record ScheduleDiagnosticOrderCommand(
    Guid OrderId)
    : ICommand<Result<DiagnosticOrderDto>>;

public sealed record PerformDiagnosticOrderCommand(
    Guid OrderId)
    : ICommand<Result<DiagnosticOrderDto>>;

public sealed record ReportDiagnosticOrderCommand(
    Guid OrderId,
    string Report)
    : ICommand<Result<DiagnosticOrderDto>>;

public sealed record CancelDiagnosticOrderCommand(
    Guid OrderId,
    string Reason)
    : ICommand<Result<DiagnosticOrderDto>>;
