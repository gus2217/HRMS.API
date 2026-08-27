using FluentValidation;
using Jacana.Billing.Application.Features.Billing;

namespace Jacana.Billing.Application.Validation;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ServiceCode).NotEmpty().MaximumLength(64);
            line.RuleFor(l => l.Description).NotEmpty().MaximumLength(300);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.AmountPaid).GreaterThan(0);
        RuleFor(x => x.ProviderTransactionReference).NotEmpty();
    }
}

public sealed class SubmitShaClaimCommandValidator : AbstractValidator<SubmitShaClaimCommand>
{
    public SubmitShaClaimCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.ShaClaimReference).NotEmpty();
    }
}

public sealed class RecordShaCallbackCommandValidator : AbstractValidator<RecordShaCallbackCommand>
{
    public RecordShaCallbackCommandValidator() => RuleFor(x => x.ShaClaimReference).NotEmpty();
}

public sealed class GetInvoiceQueryValidator : AbstractValidator<GetInvoiceQuery>
{
    public GetInvoiceQueryValidator() => RuleFor(x => x.InvoiceId).NotEmpty();
}
