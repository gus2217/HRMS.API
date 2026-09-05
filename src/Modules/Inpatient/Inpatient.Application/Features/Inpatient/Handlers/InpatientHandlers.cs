using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Inpatient.Application.Features.Inpatient.Handlers;

// ── Wards ──────────────────────────────────────────────────────────────────────

public sealed class CreateWardCommandHandler(
    IWardRepository wards,
    ICurrentUser currentUser)
    : IRequestHandler<CreateWardCommand, Result<WardDto>>
{
    public async Task<Result<WardDto>> Handle(CreateWardCommand request, CancellationToken ct)
    {
        var ward = Ward.Create(currentUser.FacilityId, request.Name, request.Type, request.TotalBeds);
        if (ward.IsFailure) return ward.Error;

        await wards.AddAsync(ward.Value, ct);
        return MapWard(ward.Value);
    }

    internal static WardDto MapWard(Ward w) =>
        new(w.Id, w.Name, w.Type.ToString(), w.TotalBeds, w.IsActive);
}

public sealed class UpdateWardCommandHandler(IWardRepository wards)
    : IRequestHandler<UpdateWardCommand, Result<WardDto>>
{
    public async Task<Result<WardDto>> Handle(UpdateWardCommand request, CancellationToken ct)
    {
        var ward = await wards.GetByIdAsync(request.WardId, ct);
        if (ward is null) return Error.NotFound("Ward not found.");

        var result = ward.Update(request.Name, request.Type, request.TotalBeds);
        if (result.IsFailure) return result.Error;

        await wards.UpdateAsync(ward, ct);
        return CreateWardCommandHandler.MapWard(ward);
    }
}

public sealed class DeactivateWardCommandHandler(IWardRepository wards)
    : IRequestHandler<DeactivateWardCommand, Result<WardDto>>
{
    public async Task<Result<WardDto>> Handle(DeactivateWardCommand request, CancellationToken ct)
    {
        var ward = await wards.GetByIdAsync(request.WardId, ct);
        if (ward is null) return Error.NotFound("Ward not found.");

        ward.Deactivate();
        await wards.UpdateAsync(ward, ct);
        return CreateWardCommandHandler.MapWard(ward);
    }
}

public sealed class ReactivateWardCommandHandler(IWardRepository wards)
    : IRequestHandler<ReactivateWardCommand, Result<WardDto>>
{
    public async Task<Result<WardDto>> Handle(ReactivateWardCommand request, CancellationToken ct)
    {
        var ward = await wards.GetByIdAsync(request.WardId, ct);
        if (ward is null) return Error.NotFound("Ward not found.");

        ward.Reactivate();
        await wards.UpdateAsync(ward, ct);
        return CreateWardCommandHandler.MapWard(ward);
    }
}

public sealed class GetWardsQueryHandler(IWardRepository wards)
    : IRequestHandler<GetWardsQuery, Result<IReadOnlyList<WardDto>>>
{
    public async Task<Result<IReadOnlyList<WardDto>>> Handle(GetWardsQuery request, CancellationToken ct)
        => Result.Success(await wards.ListAsync(request.ActiveOnly, ct));
}

// ── Admissions ─────────────────────────────────────────────────────────────────

public sealed class AdmitPatientCommandHandler(
    IAdmissionRepository admissions,
    IWardRepository wards,
    ICurrentUser currentUser,
    IClock clock,
    IUserIdentityLookup users)
    : IRequestHandler<AdmitPatientCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(AdmitPatientCommand request, CancellationToken ct)
    {
        var ward = await wards.GetByIdAsync(request.WardId, ct);
        if (ward is null) return Error.NotFound("Ward not found.");
        if (!ward.IsActive) return Error.InvalidOperation($"Ward '{ward.Name}' is inactive.");

        var occupied = await admissions.GetOccupiedBedCountAsync(ward.Id, ct);
        if (occupied >= ward.TotalBeds)
            return Error.InvalidOperation(
                $"Ward '{ward.Name}' is full ({occupied}/{ward.TotalBeds} beds occupied).");

        var admission = Admission.Admit(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId,
            request.AdmittingClinicianUserId, ward.Id, ward.Name, request.BedNumber,
            request.AdmittingDiagnosis, request.AttendingClinicianUserId, clock.UtcNow);
        if (admission.IsFailure) return admission.Error;

        await admissions.AddAsync(admission.Value, ct);
        return await AdmissionDetailEnricher.EnrichAsync(AdmissionMapper.ToDetail(admission.Value), users, ct);
    }
}

public sealed class DischargePatientCommandHandler(
    IAdmissionRepository admissions,
    IBillingStatusLookup billing,
    IClock clock,
    IUserIdentityLookup users)
    : IRequestHandler<DischargePatientCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(DischargePatientCommand request, CancellationToken ct)
    {
        var admission = await admissions.GetByIdAsync(request.AdmissionId, ct);
        if (admission is null) return Error.NotFound("Admission not found.");

        var billCleared = await billing.IsBillClearedAsync(admission.PatientId, ct);
        var result = admission.Discharge(billCleared, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await admissions.UpdateAsync(admission, ct);
        return await AdmissionDetailEnricher.EnrichAsync(AdmissionMapper.ToDetail(admission), users, ct);
    }
}

public sealed class AddWardNoteCommandHandler(
    IAdmissionRepository admissions,
    ICurrentUser currentUser,
    IClock clock,
    IUserIdentityLookup users)
    : IRequestHandler<AddWardNoteCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(AddWardNoteCommand request, CancellationToken ct)
    {
        var admission = await admissions.GetByIdAsync(request.AdmissionId, ct);
        if (admission is null) return Error.NotFound("Admission not found.");

        var result = admission.AddWardNote(request.Content, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await admissions.UpdateAsync(admission, ct);
        return await AdmissionDetailEnricher.EnrichAsync(AdmissionMapper.ToDetail(admission), users, ct);
    }
}

public sealed class TransferPatientCommandHandler(
    IAdmissionRepository admissions,
    IWardRepository wards,
    IClock clock,
    IUserIdentityLookup users)
    : IRequestHandler<TransferPatientCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(TransferPatientCommand request, CancellationToken ct)
    {
        var admission = await admissions.GetByIdAsync(request.AdmissionId, ct);
        if (admission is null) return Error.NotFound("Admission not found.");

        var target = await wards.GetByIdAsync(request.TargetWardId, ct);
        if (target is null) return Error.NotFound("Ward not found.");
        if (!target.IsActive) return Error.InvalidOperation($"Ward '{target.Name}' is inactive.");

        // Capacity check on the target ward (the admission still occupies its old
        // ward, so it is not counted in the target's occupancy yet).
        var occupied = await admissions.GetOccupiedBedCountAsync(target.Id, ct);
        if (occupied >= target.TotalBeds)
            return Error.InvalidOperation(
                $"Ward '{target.Name}' is full ({occupied}/{target.TotalBeds} beds occupied).");

        var result = admission.Transfer(target.Id, target.Name, request.BedNumber, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await admissions.UpdateAsync(admission, ct);
        return await AdmissionDetailEnricher.EnrichAsync(AdmissionMapper.ToDetail(admission), users, ct);
    }
}

/// <summary>Records a day-to-day SOAP ward medical record (with vitals).</summary>
public sealed class AddMedicalRecordCommandHandler(
    IAdmissionRepository admissions,
    ICurrentUser currentUser,
    IClock clock,
    IUserIdentityLookup users)
    : IRequestHandler<AddMedicalRecordCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(AddMedicalRecordCommand request, CancellationToken ct)
    {
        var admission = await admissions.GetByIdAsync(request.AdmissionId, ct);
        if (admission is null) return Error.NotFound("Admission not found.");

        var record = WardMedicalRecord.Create(
            admission.Id, currentUser.UserId, clock.UtcNow,
            request.TemperatureCelsius, request.SystolicBp, request.DiastolicBp,
            request.PulseRate, request.RespiratoryRate, request.OxygenSaturation,
            request.WeightKg, request.Subjective, request.Objective,
            request.Assessment, request.Plan);
        if (record.IsFailure) return record.Error;

        var result = admission.AddMedicalRecord(record.Value);
        if (result.IsFailure) return result.Error;

        await admissions.UpdateAsync(admission, ct);
        return await AdmissionDetailEnricher.EnrichAsync(AdmissionMapper.ToDetail(admission), users, ct);
    }
}

/// <summary>Attaches a media/image file to a ward medical record (stored via IFileStorage).</summary>
public sealed class AttachMedicalRecordFileCommandHandler(
    IAdmissionRepository admissions,
    IFileStorage fileStorage,
    ICurrentUser currentUser,
    IClock clock,
    IUserIdentityLookup users)
    : IRequestHandler<AttachMedicalRecordFileCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(AttachMedicalRecordFileCommand request, CancellationToken ct)
    {
        // Find the admission owning this medical record.
        var admission = await FindByMedicalRecordAsync(request.MedicalRecordId, ct);
        if (admission is null) return Error.NotFound("Medical record not found.");
        if (admission.Status == AdmissionStatus.Discharged)
            return Error.InvalidOperation("Cannot attach files to a discharged admission.");

        var record = admission.MedicalRecords.FirstOrDefault(r => r.Id == request.MedicalRecordId);
        if (record is null) return Error.NotFound("Medical record not found.");

        var attachmentId = Guid.NewGuid();
        var storageKey = $"ward/{admission.Id:N}/{request.MedicalRecordId:N}/{attachmentId:N}/{UploadAttachmentCommandHandler.Sanitize(request.FileName)}";

        var attachment = WardRecordAttachment.Create(
            record.Id, request.FileName, request.ContentType, request.Content.Length,
            storageKey, currentUser.UserId, clock.UtcNow);
        if (attachment.IsFailure) return attachment.Error;

        await fileStorage.SaveAsync(storageKey, request.Content, ct);
        var attach = record.Attach(attachment.Value);
        if (attach.IsFailure) return attach.Error;

        await admissions.UpdateAsync(admission, ct);
        return await AdmissionDetailEnricher.EnrichAsync(AdmissionMapper.ToDetail(admission), users, ct);
    }

    private async Task<Admission?> FindByMedicalRecordAsync(Guid medicalRecordId, CancellationToken ct)
    {
        // The repository loads medical records with every GetById; scan recent
        // admissions is wasteful, so we query via the admission list approach:
        // the repository's GetByIdAsync includes MedicalRecords, but we don't know
        // the admission id. Instead we search active admissions lazily here — the
        // number of active admissions is small, so a full scan is acceptable.
        // (A dedicated indexed lookup would be the production refinement.)
        const int pageSize = 50;
        for (var page = 1; ; page++)
        {
            var items = await admissions.SearchAsync(true, null, page, pageSize, ct);
            foreach (var summary in items)
            {
                var admission = await admissions.GetByIdAsync(summary.Id, ct);
                if (admission is not null &&
                    admission.MedicalRecords.Any(r => r.Id == medicalRecordId))
                    return admission;
            }
            if (items.Count < pageSize) break;
        }
        return null;
    }
}

public sealed class GetAdmissionQueryHandler(IAdmissionRepository admissions, IUserIdentityLookup users)
    : IRequestHandler<GetAdmissionQuery, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(GetAdmissionQuery request, CancellationToken ct)
    {
        var detail = await admissions.GetDetailAsync(request.AdmissionId, ct);
        if (detail is null) return Error.NotFound("Admission not found.");
        return await AdmissionDetailEnricher.EnrichAsync(detail, users, ct);
    }
}

public sealed class GetWardOccupancyQueryHandler(IAdmissionRepository admissions)
    : IRequestHandler<GetWardOccupancyQuery, Result<IReadOnlyList<WardOccupancyDto>>>
{
    public async Task<Result<IReadOnlyList<WardOccupancyDto>>> Handle(GetWardOccupancyQuery request, CancellationToken ct)
        => Result.Success(await admissions.GetWardOccupancyAsync(ct));
}

public sealed class SearchAdmissionsQueryHandler(
    IAdmissionRepository admissions,
    IPatientIdentityLookup patients)
    : IRequestHandler<SearchAdmissionsQuery, Result<PagedResult<AdmissionListItemDto>>>
{
    public async Task<Result<PagedResult<AdmissionListItemDto>>> Handle(
        SearchAdmissionsQuery request, CancellationToken ct)
    {
        var items = await admissions.SearchAsync(
            request.ActiveOnly, request.PatientId, request.PageNumber, request.PageSize, ct);
        var total = await admissions.CountAsync(request.ActiveOnly, request.PatientId, ct);

        var identities = await patients.GetIdentitiesAsync(
            items.Select(i => i.PatientId).ToArray(), ct);

        var rows = items.Select(a =>
        {
            identities.TryGetValue(a.PatientId, out var patient);
            return new AdmissionListItemDto(
                a.Id, a.PatientId,
                patient?.PatientNumber ?? string.Empty,
                patient?.FullName ?? string.Empty,
                a.WardId, a.WardName, a.BedNumber, a.Status, a.AdmittedAtUtc, null);
        }).ToArray();

        return Result.Success(new PagedResult<AdmissionListItemDto>(
            rows, total, request.PageNumber, request.PageSize));
    }
}

internal static class UploadAttachmentCommandHandler
{
    public static string Sanitize(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(fileName.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "file" : safe;
    }
}
