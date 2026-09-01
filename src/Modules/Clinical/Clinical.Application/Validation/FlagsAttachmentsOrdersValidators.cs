using FluentValidation;
using Jacana.Clinical.Application.Features.PatientClinical;

namespace Jacana.Clinical.Application.Validation;

public sealed class RaisePatientFlagCommandValidator : AbstractValidator<RaisePatientFlagCommand>
{
    public RaisePatientFlagCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
    }
}

public sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Category).MaximumLength(64);
        RuleFor(x => x.Content).NotNull();
    }
}

public sealed class CreateDiagnosticOrderCommandValidator : AbstractValidator<CreateDiagnosticOrderCommand>
{
    public CreateDiagnosticOrderCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BodySite).MaximumLength(100);
        RuleFor(x => x.ClinicalIndication).NotEmpty().MaximumLength(2000);
    }
}

public sealed class ReportDiagnosticOrderCommandValidator : AbstractValidator<ReportDiagnosticOrderCommand>
{
    public ReportDiagnosticOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Report).NotEmpty().MaximumLength(8000);
    }
}
