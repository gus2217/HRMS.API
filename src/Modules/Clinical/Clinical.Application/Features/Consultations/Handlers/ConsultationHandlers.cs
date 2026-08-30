using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.Consultations.Handlers;

public sealed class StartConsultationCommandHandler(
    IConsultationRepository consultations,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<StartConsultationCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(StartConsultationCommand request, CancellationToken ct)
    {
        var consultation = Consultation.Start(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId, request.ClinicianUserId, clock.UtcNow);
        if (consultation.IsFailure) return consultation.Error;

        await consultations.AddAsync(consultation.Value, ct);
        // Map from the in-memory aggregate — the unit-of-work transaction has not
        // committed yet, so a re-query would not see the new row.
        return ConsultationMapper.ToDetail(consultation.Value);
    }
}

public sealed class RecordTriageCommandHandler(IConsultationRepository consultations)
    : IRequestHandler<RecordTriageCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(RecordTriageCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var triage = TriageData.Create(request.TemperatureCelsius, request.BloodPressure,
            request.PulseRate, request.RespiratoryRate, request.WeightKg);
        if (triage.IsFailure) return triage.Error;

        var result = consultation.RecordTriage(triage.Value);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class BeginClinicalPhaseCommandHandler(IConsultationRepository consultations)
    : IRequestHandler<BeginClinicalPhaseCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(BeginClinicalPhaseCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var result = consultation.BeginClinicalPhase();
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class RecordDiagnosisCommandHandler(IConsultationRepository consultations)
    : IRequestHandler<RecordDiagnosisCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(RecordDiagnosisCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var result = consultation.RecordDiagnosis(request.IcdCode, request.Description, request.IsPrimary);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class AddClinicalNoteCommandHandler(
    IConsultationRepository consultations,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<AddClinicalNoteCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(AddClinicalNoteCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var result = consultation.AddClinicalNote(request.Content, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class SaveDocumentationCommandHandler(
    IConsultationRepository consultations,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<SaveDocumentationCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(SaveDocumentationCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var data = new ClinicalDocumentationData(
            request.Data.ChiefComplaint,
            request.Data.HistoryOfPresentingIllness,
            request.Data.PastMedicalHistory,
            request.Data.PastSurgicalHistory,
            request.Data.FamilyHistory,
            request.Data.SocialHistory,
            request.Data.GynaecologicalHistory,
            request.Data.ObstetricHistory,
            request.Data.DrugHistory,
            request.Data.RosGeneral,
            request.Data.RosCardiovascular,
            request.Data.RosRespiratory,
            request.Data.RosGastrointestinal,
            request.Data.RosGenitourinary,
            request.Data.RosMusculoskeletal,
            request.Data.RosNeurological,
            request.Data.RosDermatological,
            request.Data.RosEntEyes,
            request.Data.RosEndocrine,
            request.Data.ExamGeneralAppearance,
            request.Data.ExamHeadAndNeck,
            request.Data.ExamCardiovascular,
            request.Data.ExamRespiratory,
            request.Data.ExamAbdominal,
            request.Data.ExamGenitourinary,
            request.Data.ExamMusculoskeletal,
            request.Data.ExamNeurological,
            request.Data.ExamSkin,
            request.Data.ExamLymphatic);

        var result = consultation.SaveDocumentation(data, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class CreateReferralCommandHandler(
    IConsultationRepository consultations,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CreateReferralCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(CreateReferralCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var result = consultation.AddReferral(
            request.ReferredToFacility, request.ReferredToUnit, request.Reason,
            request.Priority, request.Notes, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class AttachLabOrderCommandHandler(IConsultationRepository consultations)
    : IRequestHandler<AttachLabOrderCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(AttachLabOrderCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var result = consultation.AttachLabOrder(request.LabOrderId, request.StatusSnapshot);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}

public sealed class CompleteConsultationCommandHandler(
    IConsultationRepository consultations,
    IClock clock)
    : IRequestHandler<CompleteConsultationCommand, Result<ConsultationDetailDto>>
{
    public async Task<Result<ConsultationDetailDto>> Handle(CompleteConsultationCommand request, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(request.ConsultationId, ct);
        if (consultation is null) return Error.NotFound("Consultation not found.");

        var result = consultation.Complete(clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await consultations.UpdateAsync(consultation, ct);
        return ConsultationMapper.ToDetail(consultation);
    }
}
