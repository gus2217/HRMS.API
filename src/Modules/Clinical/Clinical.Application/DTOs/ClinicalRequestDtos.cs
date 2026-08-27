namespace Jacana.Clinical.Application.DTOs;

// HTTP request bindings for the clinical endpoints (framework-agnostic records).

public sealed record StartConsultationRequestDto(Guid PatientId, Guid ClinicianUserId);

public sealed record RecordTriageRequestDto(
    decimal? TemperatureCelsius,
    string? BloodPressure,
    int? PulseRate,
    int? RespiratoryRate,
    decimal? WeightKg);

public sealed record RecordDiagnosisRequestDto(string IcdCode, string Description, bool IsPrimary);

public sealed record AddClinicalNoteRequestDto(string Content);
