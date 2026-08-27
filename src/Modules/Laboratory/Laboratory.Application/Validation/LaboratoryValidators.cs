using FluentValidation;
using Jacana.Laboratory.Application.Features.Laboratory;

namespace Jacana.Laboratory.Application.Validation;

public sealed class CreateLabOrderCommandValidator : AbstractValidator<CreateLabOrderCommand>
{
    public CreateLabOrderCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Tests).NotEmpty();
        RuleForEach(x => x.Tests).ChildRules(t =>
        {
            t.RuleFor(i => i.TestCode).NotEmpty().MaximumLength(32);
            t.RuleFor(i => i.TestName).NotEmpty().MaximumLength(150);
        });
    }
}

public sealed class RecordLabResultCommandValidator : AbstractValidator<RecordLabResultCommand>
{
    public RecordLabResultCommandValidator()
    {
        RuleFor(x => x.LabOrderId).NotEmpty();
        RuleFor(x => x.TestItemId).NotEmpty();
    }
}

public sealed class GetLabOrderQueryValidator : AbstractValidator<GetLabOrderQuery>
{
    public GetLabOrderQueryValidator() => RuleFor(x => x.LabOrderId).NotEmpty();
}
