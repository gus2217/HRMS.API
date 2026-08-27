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
        var detail = await consultations.GetDetailAsync(consultation.Value.Id, ct);
        return detail is null ? Error.NotFound("Consultation not found after creation.") : detail;
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
        var detail = await consultations.GetDetailAsync(consultation.Id, ct);
        return detail is null ? Error.NotFound("Consultation not found.") : detail;
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
        var detail = await consultations.GetDetailAsync(consultation.Id, ct);
        return detail is null ? Error.NotFound("Consultation not found.") : detail;
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
        var detail = await consultations.GetDetailAsync(consultation.Id, ct);
        return detail is null ? Error.NotFound("Consultation not found.") : detail;
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
        var detail = await consultations.GetDetailAsync(consultation.Id, ct);
        return detail is null ? Error.NotFound("Consultation not found.") : detail;
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
        var detail = await consultations.GetDetailAsync(consultation.Id, ct);
        return detail is null ? Error.NotFound("Consultation not found.") : detail;
    }
}
