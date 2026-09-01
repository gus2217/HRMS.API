namespace Jacana.Clinical.Application.DTOs;

// HTTP request bindings for flags, attachments and diagnostic orders.

public sealed record RaisePatientFlagRequestDto(string Type, string Message);

public sealed record CreateDiagnosticOrderRequestDto(
    Guid PatientId,
    Guid? ConsultationId,
    string Type,
    string Name,
    string? BodySite,
    string ClinicalIndication,
    string Priority);

public sealed record ReportDiagnosticOrderRequestDto(string Report);

public sealed record UploadAttachmentRequestDto(string FileName, string ContentType, string Category);
