using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// Structured clinical documentation for a consultation, recorded the way a
/// medical file is laid out: Chief Complaint → History of Presenting Illness →
/// PMSHX (past medical/surgical, family, social, gynaecological, obstetric,
/// drug history) → Review of Systems → Examination → Diagnosis (separate
/// entity, recorded last). Every field is nullable so the document can be
/// autosaved progressively; nothing is required until the clinician completes.
/// </summary>
public sealed class ClinicalDocumentation : Entity<Guid>
{
    private ClinicalDocumentation() { } // EF

    private ClinicalDocumentation(
        Guid id,
        Guid consultationId,
        ClinicalDocumentationData data)
        : base(id)
    {
        ConsultationId = consultationId;
        Apply(data);
    }

    public Guid ConsultationId { get; private set; }

    // ── Chief complaint + HPI ────────────────────────────────────────────────
    public string? ChiefComplaint { get; private set; }
    public string? HistoryOfPresentingIllness { get; private set; }

    // ── PMSHX ────────────────────────────────────────────────────────────────
    public string? PastMedicalHistory { get; private set; }
    public string? PastSurgicalHistory { get; private set; }
    public string? FamilyHistory { get; private set; }
    public string? SocialHistory { get; private set; }
    public string? GynaecologicalHistory { get; private set; }
    public string? ObstetricHistory { get; private set; }
    public string? DrugHistory { get; private set; }

    // ── Review of systems ────────────────────────────────────────────────────
    public string? RosGeneral { get; private set; }
    public string? RosCardiovascular { get; private set; }
    public string? RosRespiratory { get; private set; }
    public string? RosGastrointestinal { get; private set; }
    public string? RosGenitourinary { get; private set; }
    public string? RosMusculoskeletal { get; private set; }
    public string? RosNeurological { get; private set; }
    public string? RosDermatological { get; private set; }
    public string? RosEntEyes { get; private set; }
    public string? RosEndocrine { get; private set; }

    // ── Examination ──────────────────────────────────────────────────────────
    public string? ExamGeneralAppearance { get; private set; }
    public string? ExamHeadAndNeck { get; private set; }
    public string? ExamCardiovascular { get; private set; }
    public string? ExamRespiratory { get; private set; }
    public string? ExamAbdominal { get; private set; }
    public string? ExamGenitourinary { get; private set; }
    public string? ExamMusculoskeletal { get; private set; }
    public string? ExamNeurological { get; private set; }
    public string? ExamSkin { get; private set; }
    public string? ExamLymphatic { get; private set; }

    // ── Autosave metadata ────────────────────────────────────────────────────
    public DateTime? LastSavedAtUtc { get; private set; }
    public Guid? LastSavedByUserId { get; private set; }

    public static ClinicalDocumentation Create(Guid consultationId, ClinicalDocumentationData data)
        => new(Guid.NewGuid(), consultationId, data);

    public void Update(ClinicalDocumentationData data) => Apply(data);

    public void MarkSaved(Guid authorUserId, DateTime savedAtUtc)
    {
        LastSavedByUserId = authorUserId;
        LastSavedAtUtc = savedAtUtc;
    }

    private void Apply(ClinicalDocumentationData data)
    {
        ChiefComplaint = data.ChiefComplaint;
        HistoryOfPresentingIllness = data.HistoryOfPresentingIllness;

        PastMedicalHistory = data.PastMedicalHistory;
        PastSurgicalHistory = data.PastSurgicalHistory;
        FamilyHistory = data.FamilyHistory;
        SocialHistory = data.SocialHistory;
        GynaecologicalHistory = data.GynaecologicalHistory;
        ObstetricHistory = data.ObstetricHistory;
        DrugHistory = data.DrugHistory;

        RosGeneral = data.RosGeneral;
        RosCardiovascular = data.RosCardiovascular;
        RosRespiratory = data.RosRespiratory;
        RosGastrointestinal = data.RosGastrointestinal;
        RosGenitourinary = data.RosGenitourinary;
        RosMusculoskeletal = data.RosMusculoskeletal;
        RosNeurological = data.RosNeurological;
        RosDermatological = data.RosDermatological;
        RosEntEyes = data.RosEntEyes;
        RosEndocrine = data.RosEndocrine;

        ExamGeneralAppearance = data.ExamGeneralAppearance;
        ExamHeadAndNeck = data.ExamHeadAndNeck;
        ExamCardiovascular = data.ExamCardiovascular;
        ExamRespiratory = data.ExamRespiratory;
        ExamAbdominal = data.ExamAbdominal;
        ExamGenitourinary = data.ExamGenitourinary;
        ExamMusculoskeletal = data.ExamMusculoskeletal;
        ExamNeurological = data.ExamNeurological;
        ExamSkin = data.ExamSkin;
        ExamLymphatic = data.ExamLymphatic;
    }
}

/// <summary>
/// Plain data carrier for a clinical documentation section set. Lives in the
/// domain so the aggregate can accept it without depending on Application DTOs.
/// </summary>
public sealed record ClinicalDocumentationData(
    string? ChiefComplaint,
    string? HistoryOfPresentingIllness,
    string? PastMedicalHistory,
    string? PastSurgicalHistory,
    string? FamilyHistory,
    string? SocialHistory,
    string? GynaecologicalHistory,
    string? ObstetricHistory,
    string? DrugHistory,
    string? RosGeneral,
    string? RosCardiovascular,
    string? RosRespiratory,
    string? RosGastrointestinal,
    string? RosGenitourinary,
    string? RosMusculoskeletal,
    string? RosNeurological,
    string? RosDermatological,
    string? RosEntEyes,
    string? RosEndocrine,
    string? ExamGeneralAppearance,
    string? ExamHeadAndNeck,
    string? ExamCardiovascular,
    string? ExamRespiratory,
    string? ExamAbdominal,
    string? ExamGenitourinary,
    string? ExamMusculoskeletal,
    string? ExamNeurological,
    string? ExamSkin,
    string? ExamLymphatic);
