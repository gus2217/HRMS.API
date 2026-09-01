using FluentValidation;
using Jacana.Inpatient.Application.Features.Inpatient;

namespace Jacana.Inpatient.Application.Validation;

public sealed class CreateWardCommandValidator : AbstractValidator<CreateWardCommand>
{
    public CreateWardCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TotalBeds).GreaterThan(0).LessThanOrEqualTo(1000);
    }
}

public sealed class UpdateWardCommandValidator : AbstractValidator<UpdateWardCommand>
{
    public UpdateWardCommandValidator()
    {
        RuleFor(x => x.WardId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TotalBeds).GreaterThan(0).LessThanOrEqualTo(1000);
    }
}

public sealed class DeactivateWardCommandValidator : AbstractValidator<DeactivateWardCommand>
{
    public DeactivateWardCommandValidator() => RuleFor(x => x.WardId).NotEmpty();
}

public sealed class AdmitPatientCommandValidator : AbstractValidator<AdmitPatientCommand>
{
    public AdmitPatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.AdmittingClinicianUserId).NotEmpty();
        RuleFor(x => x.WardId).NotEmpty();
        RuleFor(x => x.BedNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AdmittingDiagnosis).MaximumLength(2000);
    }
}

public sealed class DischargePatientCommandValidator : AbstractValidator<DischargePatientCommand>
{
    public DischargePatientCommandValidator() => RuleFor(x => x.AdmissionId).NotEmpty();
}

public sealed class AddWardNoteCommandValidator : AbstractValidator<AddWardNoteCommand>
{
    public AddWardNoteCommandValidator()
    {
        RuleFor(x => x.AdmissionId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}

public sealed class AddMedicalRecordCommandValidator : AbstractValidator<AddMedicalRecordCommand>
{
    public AddMedicalRecordCommandValidator()
    {
        RuleFor(x => x.AdmissionId).NotEmpty();
        RuleFor(x => x.TemperatureCelsius).InclusiveBetween(25m, 45m).When(x => x.TemperatureCelsius.HasValue);
        RuleFor(x => x.SystolicBp).InclusiveBetween(0, 400).When(x => x.SystolicBp.HasValue);
        RuleFor(x => x.DiastolicBp).InclusiveBetween(0, 300).When(x => x.DiastolicBp.HasValue);
        RuleFor(x => x.PulseRate).InclusiveBetween(0, 300).When(x => x.PulseRate.HasValue);
        RuleFor(x => x.RespiratoryRate).InclusiveBetween(0, 100).When(x => x.RespiratoryRate.HasValue);
        RuleFor(x => x.OxygenSaturation).InclusiveBetween(0, 100).When(x => x.OxygenSaturation.HasValue);
        RuleFor(x => x.WeightKg).InclusiveBetween(0m, 500m).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Subjective).MaximumLength(8000);
        RuleFor(x => x.Objective).MaximumLength(8000);
        RuleFor(x => x.Assessment).MaximumLength(8000);
        RuleFor(x => x.Plan).MaximumLength(8000);
    }
}

public sealed class AttachMedicalRecordFileCommandValidator : AbstractValidator<AttachMedicalRecordFileCommand>
{
    public AttachMedicalRecordFileCommandValidator()
    {
        RuleFor(x => x.MedicalRecordId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Content).NotNull();
    }
}

public sealed class GetAdmissionQueryValidator : AbstractValidator<GetAdmissionQuery>
{
    public GetAdmissionQueryValidator() => RuleFor(x => x.AdmissionId).NotEmpty();
}
