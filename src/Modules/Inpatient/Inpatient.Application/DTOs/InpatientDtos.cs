namespace Jacana.Inpatient.Application.DTOs;

public sealed record WardNoteDto(string Content, Guid AuthorUserId, DateTime RecordedAtUtc);

public sealed record AdmissionSummaryDto(
    Guid Id,
    Guid PatientId,
    string WardName,
    string BedNumber,
    string Status,
    DateTime AdmittedAtUtc);

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
