using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>
/// An inpatient admission. Coupled to Clinical only via domain events — never a shared table.
/// </summary>
public sealed class Admission : AggregateRoot<Guid>
{
    private readonly List<WardNote> _notes = new();

    private Admission() { } // EF

    private Admission(Guid id, FacilityId facilityId, Guid patientId, Guid admittingClinicianUserId,
        string wardName, string bedNumber, DateTime admittedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        AdmittingClinicianUserId = admittingClinicianUserId;
        WardName = wardName;
        BedNumber = bedNumber;
        AdmittedAtUtc = admittedAtUtc;
        Status = AdmissionStatus.Admitted;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid AdmittingClinicianUserId { get; private set; }
    public string WardName { get; private set; } = string.Empty;
    public string BedNumber { get; private set; } = string.Empty;
    public AdmissionStatus Status { get; private set; }
    public DateTime AdmittedAtUtc { get; private set; }
    public DateTime? DischargedAtUtc { get; private set; }

    public IReadOnlyCollection<WardNote> Notes => _notes.AsReadOnly();

    public static Result<Admission> Admit(
        Guid id, FacilityId facilityId, Guid patientId, Guid admittingClinicianUserId,
        string wardName, string bedNumber, DateTime admittedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (admittingClinicianUserId == Guid.Empty) return Error.Validation("Admitting clinician is required.");
        if (string.IsNullOrWhiteSpace(wardName)) return Error.Validation("Ward name is required.");
        if (string.IsNullOrWhiteSpace(bedNumber)) return Error.Validation("Bed number is required.");

        var admission = new Admission(id, facilityId, patientId, admittingClinicianUserId,
            wardName.Trim(), bedNumber.Trim(), admittedAtUtc);
        admission.AddDomainEvent(new PatientAdmittedDomainEvent(id, patientId, wardName.Trim(), admittedAtUtc));
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

    public Result Discharge(DateTime dischargedAtUtc)
    {
        if (Status == AdmissionStatus.Discharged)
            return Error.InvalidOperation("Admission is already discharged.");

        Status = AdmissionStatus.Discharged;
        DischargedAtUtc = dischargedAtUtc;
        AddDomainEvent(new PatientDischargedDomainEvent(Id, PatientId, dischargedAtUtc));
        return Result.Success();
    }
}
