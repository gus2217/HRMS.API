using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A clinical consultation. Enforces the 7-step workflow as guarded state transitions
/// and the "cannot complete without a recorded diagnosis" rule.
/// </summary>
public sealed class Consultation : AggregateRoot<Guid>
{
    private readonly List<Diagnosis> _diagnoses = new();
    private readonly List<ClinicalNote> _notes = new();
    private readonly List<LabOrderReference> _labOrders = new();
    private readonly List<PrescriptionOrder> _prescriptionOrders = new();
    private readonly List<Referral> _referrals = new();
    private ClinicalDocumentation? _documentation;

    private Consultation() { } // EF

    private Consultation(Guid id, FacilityId facilityId, Guid patientId, Guid clinicianUserId, DateTime startedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ClinicianUserId = clinicianUserId;
        StartedAtUtc = startedAtUtc;
        Status = ConsultationStatus.Registered;
        Source = ConsultationSource.Direct;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid ClinicianUserId { get; private set; }
    public ConsultationStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public TriageData? Triage { get; private set; }

    /// <summary>How the consultation originated (walk-in, queue, appointment).</summary>
    public ConsultationSource Source { get; private set; }
    /// <summary>Links back to the queue entry / appointment that spawned this visit.</summary>
    public Guid? SourceReferenceId { get; private set; }

    public IReadOnlyCollection<Diagnosis> Diagnoses => _diagnoses.AsReadOnly();
    public IReadOnlyCollection<ClinicalNote> Notes => _notes.AsReadOnly();
    public IReadOnlyCollection<LabOrderReference> LabOrders => _labOrders.AsReadOnly();
    public IReadOnlyCollection<PrescriptionOrder> PrescriptionOrders => _prescriptionOrders.AsReadOnly();
    public IReadOnlyCollection<Referral> Referrals => _referrals.AsReadOnly();
    public ClinicalDocumentation? Documentation => _documentation;

    /// <summary>Legal forward transitions in the 7-step workflow.</summary>
    private static readonly IReadOnlyDictionary<ConsultationStatus, ConsultationStatus[]> Transitions =
        new Dictionary<ConsultationStatus, ConsultationStatus[]>
        {
            [ConsultationStatus.Registered] = [ConsultationStatus.Triaged],
            [ConsultationStatus.Triaged] = [ConsultationStatus.AwaitingClinician],
            [ConsultationStatus.AwaitingClinician] = [ConsultationStatus.InConsultation],
            [ConsultationStatus.InConsultation] =
                [ConsultationStatus.AwaitingLabResults, ConsultationStatus.DiagnosisRecorded],
            [ConsultationStatus.AwaitingLabResults] =
                [ConsultationStatus.InConsultation, ConsultationStatus.DiagnosisRecorded],
            [ConsultationStatus.DiagnosisRecorded] = [ConsultationStatus.Completed],
            [ConsultationStatus.Completed] = []
        };

    public static Result<Consultation> Start(
        Guid id, FacilityId facilityId, Guid patientId, Guid clinicianUserId, DateTime startedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (clinicianUserId == Guid.Empty) return Error.Validation("Clinician is required.");
        return new Consultation(id, facilityId, patientId, clinicianUserId, startedAtUtc);
    }

    /// <summary>Tags a consultation's origin (walk-in, queue, appointment) after start.</summary>
    public void SetSource(ConsultationSource source, Guid? sourceReferenceId)
    {
        Source = source;
        SourceReferenceId = sourceReferenceId;
    }

    public Result RecordTriage(TriageData triage)
    {
        if (Status != ConsultationStatus.Registered)
            return Error.InvalidOperation($"Cannot record triage in status {Status}.");
        Triage = triage;
        AdvanceTo(ConsultationStatus.Triaged);
        return Result.Success();
    }

    /// <summary>
    /// Moves a triaged consultation into the clinical phase (Triaged → AwaitingClinician
    /// → InConsultation) so diagnosis/complete become reachable. Guards each hop.
    /// </summary>
    public Result BeginClinicalPhase()
    {
        if (Status == ConsultationStatus.Triaged)
            AdvanceTo(ConsultationStatus.AwaitingClinician);

        if (Status == ConsultationStatus.AwaitingClinician)
            AdvanceTo(ConsultationStatus.InConsultation);

        if (Status != ConsultationStatus.InConsultation)
            return Error.InvalidOperation($"Cannot begin clinical phase in status {Status}.");

        return Result.Success();
    }

    public Result RecordDiagnosis(string icdCode, string description, bool isPrimary)
    {
        if (Status is not (ConsultationStatus.InConsultation or ConsultationStatus.AwaitingLabResults
            or ConsultationStatus.DiagnosisRecorded))
            return Error.InvalidOperation($"Cannot record a diagnosis in status {Status}.");

        var diagnosis = Diagnosis.Create(icdCode, description, isPrimary);
        if (diagnosis.IsFailure) return diagnosis.Error;
        _diagnoses.Add(diagnosis.Value);

        if (Status is ConsultationStatus.InConsultation or ConsultationStatus.AwaitingLabResults)
            AdvanceTo(ConsultationStatus.DiagnosisRecorded);

        return Result.Success();
    }

    public Result AddClinicalNote(string content, Guid authorUserId, DateTime recordedAtUtc)
    {
        if (Status == ConsultationStatus.Completed)
            return Error.InvalidOperation("Cannot add notes to a completed consultation.");

        var note = ClinicalNote.Create(content, authorUserId, recordedAtUtc);
        if (note.IsFailure) return note.Error;
        _notes.Add(note.Value);
        return Result.Success();
    }

    /// <summary>
    /// Upserts the structured clinical documentation (CC/HPI/PMSHX/ROS/Exam).
    /// Idempotent — safe for autosave: creates the document on first save,
    /// updates it thereafter. Does not change the consultation status.
    /// </summary>
    public Result SaveDocumentation(ClinicalDocumentationData data, Guid authorUserId, DateTime savedAtUtc)
    {
        if (Status == ConsultationStatus.Completed)
            return Error.InvalidOperation("Cannot edit documentation on a completed consultation.");

        _documentation ??= ClinicalDocumentation.Create(Id, data);
        _documentation.Update(data);
        _documentation.MarkSaved(authorUserId, savedAtUtc);
        return Result.Success();
    }

    public Result AddReferral(
        string referredToFacility,
        string? referredToUnit,
        string reason,
        ReferralPriority priority,
        string? notes,
        Guid referredByUserId,
        DateTime referredAtUtc)
    {
        if (Status == ConsultationStatus.Completed)
            return Error.InvalidOperation("Cannot add a referral to a completed consultation.");

        var referral = Referral.Create(
            Id, referredToFacility, referredToUnit, reason, priority, notes,
            referredByUserId, referredAtUtc);
        if (referral.IsFailure) return referral.Error;
        _referrals.Add(referral.Value);
        return Result.Success();
    }

    public Result AttachLabOrder(Guid labOrderId, string statusSnapshot)
    {
        _labOrders.Add(LabOrderReference.Create(labOrderId, statusSnapshot));
        if (Status == ConsultationStatus.InConsultation)
            AdvanceTo(ConsultationStatus.AwaitingLabResults);
        return Result.Success();
    }

    public Result AttachPrescription(Guid prescriptionId)
    {
        _prescriptionOrders.Add(PrescriptionOrder.Create(prescriptionId));
        return Result.Success();
    }

    public Result Complete(DateTime completedAtUtc)
    {
        if (Status != ConsultationStatus.DiagnosisRecorded)
            return Error.InvalidOperation($"Cannot complete a consultation in status {Status}.");
        if (_diagnoses.Count == 0)
            return Error.InvalidOperation("Cannot complete a consultation without a recorded diagnosis.");

        AdvanceTo(ConsultationStatus.Completed);
        CompletedAtUtc = completedAtUtc;
        AddDomainEvent(new ConsultationCompletedDomainEvent(Id, FacilityId.Value, PatientId, completedAtUtc));
        return Result.Success();
    }

    /// <summary>
    /// Guarded state transition. Throws <see cref="InvalidConsultationTransitionException"/>
    /// on an illegal jump (a programming/race bug, not an expected business outcome).
    /// </summary>
    public void AdvanceTo(ConsultationStatus target)
    {
        if (target == Status) return;

        if (!Transitions.TryGetValue(Status, out var allowed) || !allowed.Contains(target))
            throw new InvalidConsultationTransitionException(Status, target);

        Status = target;
    }
}
