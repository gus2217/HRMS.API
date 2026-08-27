namespace Jacana.Clinical.Domain;

/// <summary>
/// The 7-step consultation workflow. Transitions are guarded on the aggregate.
/// </summary>
public enum ConsultationStatus
{
    Registered,
    Triaged,
    AwaitingClinician,
    InConsultation,
    AwaitingLabResults,
    DiagnosisRecorded,
    Completed
}
