using FluentValidation;
using Jacana.Clinical.Application.Features.Consultations;

namespace Jacana.Clinical.Application.Validation;

public sealed class StartConsultationCommandValidator : AbstractValidator<StartConsultationCommand>
{
    public StartConsultationCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ClinicianUserId).NotEmpty();
    }
}

public sealed class RecordTriageCommandValidator : AbstractValidator<RecordTriageCommand>
{
    public RecordTriageCommandValidator() => RuleFor(x => x.ConsultationId).NotEmpty();
}

public sealed class RecordDiagnosisCommandValidator : AbstractValidator<RecordDiagnosisCommand>
{
    public RecordDiagnosisCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty();
        RuleFor(x => x.IcdCode).NotEmpty().MaximumLength(16);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public sealed class AddClinicalNoteCommandValidator : AbstractValidator<AddClinicalNoteCommand>
{
    public AddClinicalNoteCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}

public sealed class AttachLabOrderCommandValidator : AbstractValidator<AttachLabOrderCommand>
{
    public AttachLabOrderCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty();
        RuleFor(x => x.LabOrderId).NotEmpty();
        RuleFor(x => x.StatusSnapshot).NotEmpty();
    }
}

public sealed class CompleteConsultationCommandValidator : AbstractValidator<CompleteConsultationCommand>
{
    public CompleteConsultationCommandValidator() => RuleFor(x => x.ConsultationId).NotEmpty();
}

public sealed class GetConsultationQueryValidator : AbstractValidator<GetConsultationQuery>
{
    public GetConsultationQueryValidator() => RuleFor(x => x.ConsultationId).NotEmpty();
}

public sealed class GetPatientHistoryQueryValidator : AbstractValidator<GetPatientHistoryQuery>
{
    public GetPatientHistoryQueryValidator() => RuleFor(x => x.PatientId).NotEmpty();
}
