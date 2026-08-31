using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.Queue.Handlers;

/// <summary>Reception queues a patient for a consultation at a specific clinic.</summary>
public sealed class CreateQueueEntryCommandHandler(
    IQueueEntryRepository queue,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CreateQueueEntryCommand, Result<QueueEntryDto>>
{
    public async Task<Result<QueueEntryDto>> Handle(CreateQueueEntryCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<QueuePriority>(request.Priority, true, out var priority))
            return Error.Validation($"Unknown priority '{request.Priority}'.");

        // A patient should not be double-queued while they have an active entry
        // (Waiting or Accepted) — prevents duplicates from the same reception desk.
        var active = await queue.SearchAsync(request.ClinicType, null, 1, 100, ct);
        var activePatient = active.FirstOrDefault(e => e.PatientId == request.PatientId
            && (e.Status == "Waiting" || e.Status == "Accepted"));
        if (activePatient is not null)
            return Error.InvalidOperation(
                $"This patient is already {activePatient.Status} in the queue ({activePatient.QueueNumber}).");

        var sequence = await queue.NextSequenceAsync(
            currentUser.FacilityId.Value, request.ClinicType, DateOnly.FromDateTime(clock.UtcNow), ct);
        var queueNumber = $"{ClinicShortCode(request.ClinicType)}-{sequence:000}";

        var entry = QueueEntry.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId, request.ClinicType,
            priority, request.Notes, queueNumber, currentUser.UserId, clock.UtcNow);
        if (entry.IsFailure) return entry.Error;

        await queue.AddAsync(entry.Value, ct);
        return Map(entry.Value);
    }

    private static string ClinicShortCode(string clinicType) => clinicType switch
    {
        "GeneralOutpatient" => "OPD",
        "MaternalChildHealth" => "MCH",
        "Antenatal" => "ANC",
        "Postnatal" => "PNC",
        "ComprehensiveCareCentre" => "CCC",
        "Tuberculosis" => "TB",
        "FamilyPlanning" => "FP",
        "ReproductiveHealth" => "RH",
        "ChildWelfare" => "CW",
        "Immunization" => "IMZ",
        "AdolescentYouthFriendly" => "AYF",
        "Physiotherapy" => "PT",
        "Counselling" => "COUN",
        "Laboratory" => "LAB",
        "Nutrition" => "NUTR",
        "Wellness" => "WELL",
        "Dental" => "DENT",
        "Eye" => "EYE",
        "Ent" => "ENT",
        _ => clinicType.Length > 3 ? clinicType[..3].ToUpperInvariant() : clinicType.ToUpperInvariant()
    };

    private static QueueEntryDto Map(QueueEntry e) => new(
        e.Id, e.PatientId, string.Empty, string.Empty, e.ClinicType,
        e.Priority.ToString(), e.Status.ToString(), e.QueueNumber, e.Notes,
        e.RequestedByUserId, e.RequestedAtUtc, e.AcceptedByUserId, e.AcceptedAtUtc, e.ConsultationId);
}

/// <summary>
/// Clinician accepts a waiting queue entry — atomically registers the consultation
/// (same DbContext, same transaction) and links it to the entry.
/// </summary>
public sealed class AcceptQueueEntryCommandHandler(
    IQueueEntryRepository queue,
    IConsultationRepository consultations,
    ICurrentUser currentUser,
    IClock clock,
    IPatientIdentityLookup patients)
    : IRequestHandler<AcceptQueueEntryCommand, Result<AcceptQueueEntryResponseDto>>
{
    public async Task<Result<AcceptQueueEntryResponseDto>> Handle(AcceptQueueEntryCommand request, CancellationToken ct)
    {
        var entry = await queue.GetByIdAsync(request.QueueEntryId, ct);
        if (entry is null) return Error.NotFound("Queue entry not found.");
        if (entry.Status != QueueStatus.Waiting)
            return Error.InvalidOperation($"Queue entry is {entry.Status}, not waiting.");

        // Register the consultation for this patient under the accepting clinician.
        var consultation = Consultation.Start(
            Guid.NewGuid(), currentUser.FacilityId, entry.PatientId, currentUser.UserId, clock.UtcNow);
        if (consultation.IsFailure) return consultation.Error;
        consultation.Value.SetSource(ConsultationSource.Queue, entry.Id);

        var accept = entry.Accept(currentUser.UserId, consultation.Value.Id, clock.UtcNow);
        if (accept.IsFailure) return accept.Error;

        await consultations.AddAsync(consultation.Value, ct);
        await queue.UpdateAsync(entry, ct);

        var dto = await ToDtoAsync(entry, ct);
        return new AcceptQueueEntryResponseDto(dto, consultation.Value.Id);
    }

    private async Task<QueueEntryDto> ToDtoAsync(QueueEntry e, CancellationToken ct)
    {
        var identities = await patients.GetIdentitiesAsync([e.PatientId], ct);
        identities.TryGetValue(e.PatientId, out var patient);
        return new QueueEntryDto(
            e.Id, e.PatientId,
            patient?.PatientNumber ?? string.Empty,
            patient?.FullName ?? string.Empty,
            e.ClinicType, e.Priority.ToString(), e.Status.ToString(), e.QueueNumber, e.Notes,
            e.RequestedByUserId, e.RequestedAtUtc, e.AcceptedByUserId, e.AcceptedAtUtc, e.ConsultationId);
    }
}

/// <summary>Reception cancels a waiting entry (patient left, duplicate, etc.).</summary>
public sealed class CancelQueueEntryCommandHandler(
    IQueueEntryRepository queue,
    IPatientIdentityLookup patients)
    : IRequestHandler<CancelQueueEntryCommand, Result<QueueEntryDto>>
{
    public async Task<Result<QueueEntryDto>> Handle(CancelQueueEntryCommand request, CancellationToken ct)
    {
        var entry = await queue.GetByIdAsync(request.QueueEntryId, ct);
        if (entry is null) return Error.NotFound("Queue entry not found.");

        var cancel = entry.Cancel(Guid.Empty, DateTime.UtcNow);
        if (cancel.IsFailure) return cancel.Error;

        await queue.UpdateAsync(entry, ct);

        var identities = await patients.GetIdentitiesAsync([entry.PatientId], ct);
        identities.TryGetValue(entry.PatientId, out var patient);
        return new QueueEntryDto(
            entry.Id, entry.PatientId,
            patient?.PatientNumber ?? string.Empty,
            patient?.FullName ?? string.Empty,
            entry.ClinicType, entry.Priority.ToString(), entry.Status.ToString(), entry.QueueNumber, entry.Notes,
            entry.RequestedByUserId, entry.RequestedAtUtc, entry.AcceptedByUserId, entry.AcceptedAtUtc, entry.ConsultationId);
    }
}

/// <summary>Queue board — reception + clinicians, filterable by clinic and status.</summary>
public sealed class SearchQueueEntriesQueryHandler(
    IQueueEntryRepository queue,
    IPatientIdentityLookup patients)
    : IRequestHandler<SearchQueueEntriesQuery, Result<PagedResult<QueueEntryDto>>>
{
    public async Task<Result<PagedResult<QueueEntryDto>>> Handle(SearchQueueEntriesQuery request, CancellationToken ct)
    {
        var items = await queue.SearchAsync(request.ClinicType, request.Status, request.PageNumber, request.PageSize, ct);
        var total = await queue.CountAsync(request.ClinicType, request.Status, ct);

        var identities = await patients.GetIdentitiesAsync(
            items.Select(i => i.PatientId).ToArray(), ct);

        var rows = items.Select(i =>
        {
            identities.TryGetValue(i.PatientId, out var patient);
            return new QueueEntryDto(
                i.Id, i.PatientId,
                patient?.PatientNumber ?? string.Empty,
                patient?.FullName ?? string.Empty,
                i.ClinicType, i.Priority, i.Status, i.QueueNumber, i.Notes,
                i.RequestedByUserId, i.RequestedAtUtc, i.AcceptedByUserId, i.AcceptedAtUtc, i.ConsultationId);
        }).ToArray();

        return Result.Success(new PagedResult<QueueEntryDto>(rows, total, request.PageNumber, request.PageSize));
    }
}
