namespace Jacana.Clinical.Application.DTOs;

/// <summary>List-view row with patient display identity resolved cross-schema.</summary>
public sealed record ConsultationListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    Guid ClinicianUserId,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);

/// <summary>Lean read-model for list/history views.</summary>
public sealed record ConsultationSummaryDto(
    Guid Id,
    Guid PatientId,
    Guid ClinicianUserId,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);

/// <summary>Full single-record projection.</summary>
public sealed record ConsultationDetailDto(
    Guid Id,
    Guid PatientId,
    Guid ClinicianUserId,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    TriageDataDto? Triage,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalNoteDto> Notes);

public sealed record TriageDataDto(
    decimal? TemperatureCelsius,
    string? BloodPressure,
    int? PulseRate,
    int? RespiratoryRate,
    decimal? WeightKg);

public sealed record DiagnosisDto(string IcdCode, string Description, bool IsPrimary);
public sealed record ClinicalNoteDto(string Content, Guid AuthorUserId, DateTime RecordedAtUtc);

/// <summary>Read-model of a patient's clinical history across consultations.</summary>
public sealed record PatientClinicalHistoryDto(
    Guid PatientId,
    IReadOnlyList<ConsultationSummaryDto> Consultations,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalNoteDto> Notes);
