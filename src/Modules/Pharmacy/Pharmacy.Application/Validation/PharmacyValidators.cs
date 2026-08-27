using FluentValidation;
using Jacana.Pharmacy.Application.Features.Pharmacy;

namespace Jacana.Pharmacy.Application.Validation;

public sealed class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.DrugId).NotEmpty();
            item.RuleFor(i => i.DosageInstructions).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.QuantityPrescribed).GreaterThan(0);
        });
    }
}

public sealed class DispenseMedicationCommandValidator : AbstractValidator<DispenseMedicationCommand>
{
    public DispenseMedicationCommandValidator()
    {
        RuleFor(x => x.PrescriptionId).NotEmpty();
        RuleFor(x => x.PrescriptionItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class GetPrescriptionQueryValidator : AbstractValidator<GetPrescriptionQuery>
{
    public GetPrescriptionQueryValidator() => RuleFor(x => x.PrescriptionId).NotEmpty();
}
