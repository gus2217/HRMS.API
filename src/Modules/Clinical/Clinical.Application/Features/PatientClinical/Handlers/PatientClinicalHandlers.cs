using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.PatientClinical.Handlers;

public sealed class RecordVitalsCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<RecordVitalsCommand, Result<VitalSignDto>>
{
    public async Task<Result<VitalSignDto>> Handle(RecordVitalsCommand request, CancellationToken ct)
    {
        var vitals = VitalSign.Record(
            currentUser.FacilityId, request.PatientId,
            request.TemperatureCelsius, request.SystolicBp, request.DiastolicBp,
            request.PulseRate, request.RespiratoryRate, request.OxygenSaturation,
            request.WeightKg, request.HeightCm, currentUser.UserId, clock.UtcNow);
        if (vitals.IsFailure) return vitals.Error;

        await repository.AddVitalSignAsync(vitals.Value, ct);
        return MapVitals(vitals.Value);
    }

    private static VitalSignDto MapVitals(VitalSign v) => new(
        v.Id, v.PatientId, v.TemperatureCelsius, v.SystolicBp, v.DiastolicBp,
        v.PulseRate, v.RespiratoryRate, v.OxygenSaturation, v.WeightKg, v.HeightCm,
        v.Bmi, v.RecordedByUserId, v.RecordedAtUtc);
}

public sealed class RecordImmunizationCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<RecordImmunizationCommand, Result<ImmunizationDto>>
{
    public async Task<Result<ImmunizationDto>> Handle(RecordImmunizationCommand request, CancellationToken ct)
    {
        var immunization = Immunization.Record(
            currentUser.FacilityId, request.PatientId, request.VaccineName, request.DoseNumber,
            request.AdministeredDate, request.NextDueDate, request.LotNumber, request.Site,
            request.Notes, currentUser.UserId, clock.UtcNow);
        if (immunization.IsFailure) return immunization.Error;

        await repository.AddImmunizationAsync(immunization.Value, ct);
        var v = immunization.Value;
        return new ImmunizationDto(
            v.Id, v.PatientId, v.VaccineName, v.DoseNumber, v.AdministeredDate, v.NextDueDate,
            v.LotNumber, v.Site, v.Notes, v.RecordedByUserId, v.RecordedAtUtc);
    }
}

public sealed class AddConditionCommandHandler(
    IPatientClinicalRepository repository,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<AddConditionCommand, Result<ConditionDto>>
{
    public async Task<Result<ConditionDto>> Handle(AddConditionCommand request, CancellationToken ct)
    {
        var condition = Condition.Add(
            currentUser.FacilityId, request.PatientId, request.Code, request.Description,
            request.OnsetDate, currentUser.UserId, clock.UtcNow);
        if (condition.IsFailure) return condition.Error;

        await repository.AddConditionAsync(condition.Value, ct);
        return MapCondition(condition.Value);
    }

    private static ConditionDto MapCondition(Condition c) => new(
        c.Id, c.PatientId, c.Code, c.Description, c.Status.ToString(),
        c.OnsetDate, c.ResolvedDate, c.RecordedByUserId, c.RecordedAtUtc);
}

public sealed class ResolveConditionCommandHandler(IPatientClinicalRepository repository)
    : IRequestHandler<ResolveConditionCommand, Result<ConditionDto>>
{
    public async Task<Result<ConditionDto>> Handle(ResolveConditionCommand request, CancellationToken ct)
    {
        var condition = await repository.GetConditionAsync(request.ConditionId, ct);
        if (condition is null) return Error.NotFound("Condition not found.");

        var result = condition.Resolve(request.ResolvedDate);
        if (result.IsFailure) return result.Error;

        await repository.UpdateConditionAsync(condition, ct);
        return new ConditionDto(
            condition.Id, condition.PatientId, condition.Code, condition.Description,
            condition.Status.ToString(), condition.OnsetDate, condition.ResolvedDate,
            condition.RecordedByUserId, condition.RecordedAtUtc);
    }
}

public sealed class GetVitalsQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetVitalsQuery, Result<IReadOnlyList<VitalSignDto>>>
{
    public async Task<Result<IReadOnlyList<VitalSignDto>>> Handle(GetVitalsQuery request, CancellationToken ct)
        => Result.Success(await repository.GetVitalsAsync(request.PatientId, ct));
}

public sealed class GetImmunizationsQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetImmunizationsQuery, Result<IReadOnlyList<ImmunizationDto>>>
{
    public async Task<Result<IReadOnlyList<ImmunizationDto>>> Handle(GetImmunizationsQuery request, CancellationToken ct)
        => Result.Success(await repository.GetImmunizationsAsync(request.PatientId, ct));
}

public sealed class GetConditionsQueryHandler(IPatientClinicalRepository repository)
    : IRequestHandler<GetConditionsQuery, Result<IReadOnlyList<ConditionDto>>>
{
    public async Task<Result<IReadOnlyList<ConditionDto>>> Handle(GetConditionsQuery request, CancellationToken ct)
        => Result.Success(await repository.GetConditionsAsync(request.PatientId, ct));
}
