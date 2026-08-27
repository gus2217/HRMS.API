using FluentValidation;
using Jacana.Inpatient.Application.Features.Inpatient;

namespace Jacana.Inpatient.Application.Validation;

public sealed class AdmitPatientCommandValidator : AbstractValidator<AdmitPatientCommand>
{
    public AdmitPatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.AdmittingClinicianUserId).NotEmpty();
        RuleFor(x => x.WardName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BedNumber).NotEmpty().MaximumLength(50);
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

public sealed class GetAdmissionQueryValidator : AbstractValidator<GetAdmissionQuery>
{
    public GetAdmissionQueryValidator() => RuleFor(x => x.AdmissionId).NotEmpty();
}
