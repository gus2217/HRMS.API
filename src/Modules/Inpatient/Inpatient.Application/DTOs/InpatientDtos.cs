namespace Jacana.Inpatient.Application.DTOs;

public sealed record WardDto(
    Guid Id,
    string Name,
    string Type,
    int TotalBeds,
    bool IsActive);

public sealed record WardNoteDto(string Content, Guid AuthorUserId, DateTime RecordedAtUtc);

public sealed record WardRecordAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc);

public sealed record WardMedicalRecordDto(
    Guid Id,
    Guid RecordedByUserId,
    DateTime RecordedAtUtc,
    decimal? TemperatureCelsius,
    int? SystolicBp,
    int? DiastolicBp,
    int? PulseRate,
    int? RespiratoryRate,
    int? OxygenSaturation,
    decimal? WeightKg,
    string? Subjective,
    string? Objective,
    string? Assessment,
    string? Plan,
    bool IsComplete,
    IReadOnlyList<WardRecordAttachmentDto> Attachments);

public sealed record AdmissionSummaryDto(
    Guid Id,
    Guid PatientId,
    Guid WardId,
    string WardName,
    string BedNumber,
    string Status,
    DateTime AdmittedAtUtc);

/// <summary>List-view row with patient display identity resolved cross-schema.</summary>
public sealed record AdmissionListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    Guid WardId,
    string WardName,
    string BedNumber,
    string Status,
    DateTime AdmittedAtUtc,
    DateTime? DischargedAtUtc);

public sealed record AdmissionDetailDto(
    Guid Id,
    Guid PatientId,
    Guid AdmittingClinicianUserId,
    Guid WardId,
    string WardName,
    string BedNumber,
    string? AdmittingDiagnosis,
    Guid? AttendingClinicianUserId,
    string Status,
    DateTime AdmittedAtUtc,
    DateTime? DischargedAtUtc,
    IReadOnlyList<WardNoteDto> Notes,
    IReadOnlyList<WardMedicalRecordDto> MedicalRecords,
    bool HasCompleteMedicalRecord);

public sealed record WardOccupancyDto(
    Guid WardId,
    string WardName,
    int OccupiedBeds,
    int TotalBeds);
