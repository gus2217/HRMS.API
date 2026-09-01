using FluentValidation;
using Jacana.Clinical.Application.Features.PatientClinical;

namespace Jacana.Clinical.Application.Validation;

public sealed class RecordVitalsCommandValidator : AbstractValidator<RecordVitalsCommand>
{
    public RecordVitalsCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.TemperatureCelsius).InclusiveBetween(25m, 45m).When(x => x.TemperatureCelsius.HasValue);
        RuleFor(x => x.SystolicBp).InclusiveBetween(0, 400).When(x => x.SystolicBp.HasValue);
        RuleFor(x => x.DiastolicBp).InclusiveBetween(0, 300).When(x => x.DiastolicBp.HasValue);
        RuleFor(x => x.PulseRate).InclusiveBetween(0, 300).When(x => x.PulseRate.HasValue);
        RuleFor(x => x.RespiratoryRate).InclusiveBetween(0, 100).When(x => x.RespiratoryRate.HasValue);
        RuleFor(x => x.OxygenSaturation).InclusiveBetween(0, 100).When(x => x.OxygenSaturation.HasValue);
        RuleFor(x => x.WeightKg).InclusiveBetween(0m, 500m).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.HeightCm).InclusiveBetween(0m, 300m).When(x => x.HeightCm.HasValue);
    }
}

public sealed class RecordImmunizationCommandValidator : AbstractValidator<RecordImmunizationCommand>
{
    public RecordImmunizationCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.VaccineName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DoseNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.LotNumber).MaximumLength(100);
        RuleFor(x => x.Site).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class AddConditionCommandValidator : AbstractValidator<AddConditionCommand>
{
    public AddConditionCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Code).MaximumLength(16);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public sealed class ResolveConditionCommandValidator : AbstractValidator<ResolveConditionCommand>
{
    public ResolveConditionCommandValidator() => RuleFor(x => x.ConditionId).NotEmpty();
}
