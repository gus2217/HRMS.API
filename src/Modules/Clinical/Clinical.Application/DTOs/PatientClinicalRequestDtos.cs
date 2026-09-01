namespace Jacana.Clinical.Application.DTOs;

// HTTP request bindings for the patient clinical-summary endpoints.

public sealed record RecordVitalsRequestDto(
    decimal? TemperatureCelsius,
    int? SystolicBp,
    int? DiastolicBp,
    int? PulseRate,
    int? RespiratoryRate,
    int? OxygenSaturation,
    decimal? WeightKg,
    decimal? HeightCm);

public sealed record RecordImmunizationRequestDto(
    string VaccineName,
    int DoseNumber,
    DateTime AdministeredDate,
    DateTime? NextDueDate,
    string? LotNumber,
    string? Site,
    string? Notes);

public sealed record AddConditionRequestDto(
    string? Code,
    string Description,
    DateTime OnsetDate);
