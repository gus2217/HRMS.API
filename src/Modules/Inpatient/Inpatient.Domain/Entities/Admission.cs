using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>
/// An inpatient admission. Tied to a <see cref="Ward"/> (created by the
/// administrator) and enriched with the admitting diagnosis + attending clinician.
/// A patient cannot be discharged until (a) the day-to-day ward medical records
/// are complete (SOAP assessment + plan) and (b) the bill is cleared — both are
/// enforced as guarded state transitions.
/// </summary>
public sealed class Admission : AggregateRoot<Guid>
{
    private readonly List<WardNote> _notes = new();
    private readonly List<WardMedicalRecord> _medicalRecords = new();

    private Admission() { } // EF

    private Admission(
        Guid id, FacilityId facilityId, Guid patientId, Guid admittingClinicianUserId,
        Guid wardId, string wardName, string bedNumber, string? admittingDiagnosis,
        Guid? attendingClinicianUserId, DateTime admittedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        AdmittingClinicianUserId = admittingClinicianUserId;
        WardId = wardId;
        WardName = wardName;
        BedNumber = bedNumber;
        AdmittingDiagnosis = admittingDiagnosis;
        AttendingClinicianUserId = attendingClinicianUserId;
        AdmittedAtUtc = admittedAtUtc;
        Status = AdmissionStatus.Admitted;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid AdmittingClinicianUserId { get; private set; }

    /// <summary>The ward this patient is assigned to (admin-managed).</summary>
    public Guid WardId { get; private set; }
    /// <summary>Snapshot of the ward name at admission time (display convenience).</summary>
    public string WardName { get; private set; } = string.Empty;
    public string BedNumber { get; private set; } = string.Empty;
    public string? AdmittingDiagnosis { get; private set; }
    public Guid? AttendingClinicianUserId { get; private set; }

    public AdmissionStatus Status { get; private set; }
    public DateTime AdmittedAtUtc { get; private set; }
    public DateTime? DischargedAtUtc { get; private set; }

    public IReadOnlyCollection<WardNote> Notes => _notes.AsReadOnly();
    public IReadOnlyCollection<WardMedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    public static Result<Admission> Admit(
        Guid id, FacilityId facilityId, Guid patientId, Guid admittingClinicianUserId,
        Guid wardId, string wardName, string bedNumber, string? admittingDiagnosis,
        Guid? attendingClinicianUserId, DateTime admittedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (admittingClinicianUserId == Guid.Empty) return Error.Validation("Admitting clinician is required.");
        if (wardId == Guid.Empty) return Error.Validation("Ward is required.");
        if (string.IsNullOrWhiteSpace(wardName)) return Error.Validation("Ward name is required.");
        if (string.IsNullOrWhiteSpace(bedNumber)) return Error.Validation("Bed number is required.");

        var admission = new Admission(id, facilityId, patientId, admittingClinicianUserId,
            wardId, wardName.Trim(), bedNumber.Trim(),
            string.IsNullOrWhiteSpace(admittingDiagnosis) ? null : admittingDiagnosis.Trim(),
            attendingClinicianUserId, admittedAtUtc);
        admission.AddDomainEvent(new PatientAdmittedDomainEvent(id, facilityId.Value, patientId, wardName.Trim(), admittedAtUtc));
        return admission;
    }

    public Result AddWardNote(string content, Guid authorUserId, DateTime recordedAtUtc)
    {
        if (Status == AdmissionStatus.Discharged)
            return Error.InvalidOperation("Cannot add notes to a discharged admission.");

        var note = WardNote.Create(content, authorUserId, recordedAtUtc);
        if (note.IsFailure) return note.Error;
        _notes.Add(note.Value);
        return Result.Success();
    }

    /// <summary>
    /// Transfers the patient to another ward/bed. Guarded: only active admissions
    /// can be moved, and the transfer is recorded as a ward note + domain event
    /// (which the Notifications module fans out to doctors/nurses).
    /// </summary>
    public Result Transfer(Guid wardId, string wardName, string bedNumber, DateTime transferredAtUtc)
    {
        if (Status == AdmissionStatus.Discharged)
            return Error.InvalidOperation("Cannot transfer a discharged admission.");
        if (wardId == Guid.Empty) return Error.Validation("Ward is required.");
        if (string.IsNullOrWhiteSpace(wardName)) return Error.Validation("Ward name is required.");
        if (string.IsNullOrWhiteSpace(bedNumber)) return Error.Validation("Bed number is required.");

        var fromWardId = WardId;
        var fromWardName = WardName;

        WardId = wardId;
        WardName = wardName.Trim();
        BedNumber = bedNumber.Trim();

        _notes.Add(WardNote.Create(
            $"Transferred from {fromWardName} to {wardName.Trim()} (bed {bedNumber.Trim()}).",
            Guid.Empty, transferredAtUtc).Value);

        AddDomainEvent(new PatientTransferredDomainEvent(
            Id, FacilityId.Value, PatientId, fromWardId, fromWardName, wardId, wardName.Trim(), transferredAtUtc));
        return Result.Success();
    }

    /// <summary>
    /// Records a day-to-day SOAP-style ward medical record (vitals + assessment +
    /// plan). Only admitted/under-observation patients can receive new records.
    /// </summary>
    public Result AddMedicalRecord(WardMedicalRecord record)
    {
        if (Status == AdmissionStatus.Discharged)
            return Error.InvalidOperation("Cannot add medical records to a discharged admission.");
        _medicalRecords.Add(record);
        return Result.Success();
    }

    /// <summary>
    /// True once at least one ward medical record has a completed SOAP
    /// (assessment + plan) — the "medical records filled" discharge gate.
    /// </summary>
    public bool HasCompleteMedicalRecord => _medicalRecords.Any(r => r.IsComplete);

    /// <summary>
    /// Discharges the patient. Both gates must be satisfied: complete medical
    /// records and a cleared bill — otherwise an explanatory validation error is
    /// returned so the UI can show exactly what is missing.
    /// </summary>
    public Result Discharge(bool billCleared, DateTime dischargedAtUtc)
    {
        if (Status == AdmissionStatus.Discharged)
            return Error.InvalidOperation("Admission is already discharged.");

        if (!HasCompleteMedicalRecord)
            return Error.Validation(
                "Cannot discharge: ward medical records must be completed first (assessment + plan).");
        if (!billCleared)
            return Error.Validation(
                "Cannot discharge: the patient's bill has not been cleared.");

        Status = AdmissionStatus.Discharged;
        DischargedAtUtc = dischargedAtUtc;
        AddDomainEvent(new PatientDischargedDomainEvent(Id, FacilityId.Value, PatientId, dischargedAtUtc));
        return Result.Success();
    }
}
