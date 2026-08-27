using FluentValidation;
using Jacana.PatientRegistration.Application.Features.Patients;

namespace Jacana.PatientRegistration.Application.Validation;

public sealed class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.County).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.NextOfKin).ChildRules(kin =>
        {
            kin.RuleFor(k => k.FullName).NotEmpty();
            kin.RuleFor(k => k.Relationship).NotEmpty();
            kin.RuleFor(k => k.Phone).NotEmpty();
        });
    }
}

public sealed class UpdatePatientDemographicsCommandValidator : AbstractValidator<UpdatePatientDemographicsCommand>
{
    public UpdatePatientDemographicsCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.County).NotEmpty();
    }
}

public sealed class RegisterAllergyCommandValidator : AbstractValidator<RegisterAllergyCommand>
{
    public RegisterAllergyCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Substance).NotEmpty().MaximumLength(150);
    }
}

public sealed class RecordConsentCommandValidator : AbstractValidator<RecordConsentCommand>
{
    public RecordConsentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
    }
}

public sealed class GetPatientQueryValidator : AbstractValidator<GetPatientQuery>
{
    public GetPatientQueryValidator() => RuleFor(x => x.PatientId).NotEmpty();
}

public sealed class SearchPatientsQueryValidator : AbstractValidator<SearchPatientsQuery>
{
    public SearchPatientsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
