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

public sealed class SaveDocumentationCommandValidator : AbstractValidator<SaveDocumentationCommand>
{
    public SaveDocumentationCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty();
        RuleFor(x => x.Data.ChiefComplaint).MaximumLength(2000);
        RuleFor(x => x.Data.HistoryOfPresentingIllness).MaximumLength(8000);
        RuleFor(x => x.Data.PastMedicalHistory).MaximumLength(4000);
        RuleFor(x => x.Data.PastSurgicalHistory).MaximumLength(4000);
        RuleFor(x => x.Data.FamilyHistory).MaximumLength(4000);
        RuleFor(x => x.Data.SocialHistory).MaximumLength(4000);
        RuleFor(x => x.Data.GynaecologicalHistory).MaximumLength(4000);
        RuleFor(x => x.Data.ObstetricHistory).MaximumLength(4000);
        RuleFor(x => x.Data.DrugHistory).MaximumLength(4000);
        RuleFor(x => x.Data.RosGeneral).MaximumLength(4000);
        RuleFor(x => x.Data.RosCardiovascular).MaximumLength(4000);
        RuleFor(x => x.Data.RosRespiratory).MaximumLength(4000);
        RuleFor(x => x.Data.RosGastrointestinal).MaximumLength(4000);
        RuleFor(x => x.Data.RosGenitourinary).MaximumLength(4000);
        RuleFor(x => x.Data.RosMusculoskeletal).MaximumLength(4000);
        RuleFor(x => x.Data.RosNeurological).MaximumLength(4000);
        RuleFor(x => x.Data.RosDermatological).MaximumLength(4000);
        RuleFor(x => x.Data.RosEntEyes).MaximumLength(4000);
        RuleFor(x => x.Data.RosEndocrine).MaximumLength(4000);
        RuleFor(x => x.Data.ExamGeneralAppearance).MaximumLength(4000);
        RuleFor(x => x.Data.ExamHeadAndNeck).MaximumLength(4000);
        RuleFor(x => x.Data.ExamCardiovascular).MaximumLength(4000);
        RuleFor(x => x.Data.ExamRespiratory).MaximumLength(4000);
        RuleFor(x => x.Data.ExamAbdominal).MaximumLength(4000);
        RuleFor(x => x.Data.ExamGenitourinary).MaximumLength(4000);
        RuleFor(x => x.Data.ExamMusculoskeletal).MaximumLength(4000);
        RuleFor(x => x.Data.ExamNeurological).MaximumLength(4000);
        RuleFor(x => x.Data.ExamSkin).MaximumLength(4000);
        RuleFor(x => x.Data.ExamLymphatic).MaximumLength(4000);
    }
}

public sealed class CreateReferralCommandValidator : AbstractValidator<CreateReferralCommand>
{
    public CreateReferralCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty();
        RuleFor(x => x.ReferredToFacility).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReferredToUnit).MaximumLength(200);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Priority).IsInEnum();
    }
}
