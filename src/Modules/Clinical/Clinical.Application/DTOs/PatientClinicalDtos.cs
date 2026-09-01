namespace Jacana.Clinical.Application.DTOs;

/// <summary>One standalone vitals observation (trend point).</summary>
public sealed record VitalSignDto(
    Guid Id,
    Guid PatientId,
    decimal? TemperatureCelsius,
    int? SystolicBp,
    int? DiastolicBp,
    int? PulseRate,
    int? RespiratoryRate,
    int? OxygenSaturation,
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? Bmi,
    Guid RecordedByUserId,
    DateTime RecordedAtUtc);

/// <summary>One vaccination record.</summary>
public sealed record ImmunizationDto(
    Guid Id,
    Guid PatientId,
    string VaccineName,
    int DoseNumber,
    DateTime AdministeredDate,
    DateTime? NextDueDate,
    string? LotNumber,
    string? Site,
    string? Notes,
    Guid RecordedByUserId,
    DateTime RecordedAtUtc);

/// <summary>One persistent problem-list condition.</summary>
public sealed record ConditionDto(
    Guid Id,
    Guid PatientId,
    string? Code,
    string Description,
    string Status,
    DateTime OnsetDate,
    DateTime? ResolvedDate,
    Guid RecordedByUserId,
    DateTime RecordedAtUtc);
