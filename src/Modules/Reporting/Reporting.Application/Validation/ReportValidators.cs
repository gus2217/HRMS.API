using FluentValidation;
using Jacana.Reporting.Application.Features.Reporting;

namespace Jacana.Reporting.Application.Validation;

public sealed class DailyRegistrationsReportQueryValidator : AbstractValidator<DailyRegistrationsReportQuery>
{
    public DailyRegistrationsReportQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty();
        RuleFor(x => x).Must(x => x.From <= x.To).WithMessage("From date must not be after To date.");
    }
}
