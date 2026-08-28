namespace Jacana.Inpatient.Application.DTOs;

public sealed record WardNoteDto(string Content, Guid AuthorUserId, DateTime RecordedAtUtc);

public sealed record AdmissionSummaryDto(
    Guid Id,
    Guid PatientId,
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
    string WardName,
    string BedNumber,
    string Status,
    DateTime AdmittedAtUtc,
    DateTime? DischargedAtUtc);

public sealed record AdmissionDetailDto(
    Guid Id,
    Guid PatientId,
    Guid AdmittingClinicianUserId,
    string WardName,
    string BedNumber,
    string Status,
    DateTime AdmittedAtUtc,
    DateTime? DischargedAtUtc,
    IReadOnlyList<WardNoteDto> Notes);

public sealed record WardOccupancyDto(
    string WardName,
    int OccupiedBeds,
    int TotalBeds);
