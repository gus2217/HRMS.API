using FluentValidation;
using Jacana.Audit.Application.Features.Audit;

namespace Jacana.Audit.Application.Validation;

public sealed class GetAuditLogQueryValidator : AbstractValidator<GetAuditLogQuery>
{
    public GetAuditLogQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
