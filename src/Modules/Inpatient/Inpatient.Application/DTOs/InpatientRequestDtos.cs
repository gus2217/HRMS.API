namespace Jacana.Inpatient.Application.DTOs;

// HTTP request bindings for inpatient endpoints.

public sealed record CreateWardRequestDto(string Name, string Type, int TotalBeds);

public sealed record UpdateWardRequestDto(string Name, string Type, int TotalBeds);

public sealed record AdmitPatientRequestDto(
    Guid PatientId,
    Guid AdmittingClinicianUserId,
    Guid WardId,
    string BedNumber,
    string? AdmittingDiagnosis,
    Guid? AttendingClinicianUserId);

public sealed record AddWardNoteRequestDto(string Content);

public sealed record AddMedicalRecordRequestDto(
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
    string? Plan);
