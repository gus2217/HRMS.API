using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.PatientClinical;

public sealed record RecordVitalsCommand(
    Guid PatientId,
    decimal? TemperatureCelsius,
    int? SystolicBp,
    int? DiastolicBp,
    int? PulseRate,
    int? RespiratoryRate,
    int? OxygenSaturation,
    decimal? WeightKg,
    decimal? HeightCm)
    : ICommand<Result<VitalSignDto>>;

public sealed record RecordImmunizationCommand(
    Guid PatientId,
    string VaccineName,
    int DoseNumber,
    DateTime AdministeredDate,
    DateTime? NextDueDate,
    string? LotNumber,
    string? Site,
    string? Notes)
    : ICommand<Result<ImmunizationDto>>;

public sealed record AddConditionCommand(
    Guid PatientId,
    string? Code,
    string Description,
    DateTime OnsetDate)
    : ICommand<Result<ConditionDto>>;

public sealed record ResolveConditionCommand(
    Guid ConditionId,
    DateTime ResolvedDate)
    : ICommand<Result<ConditionDto>>;
