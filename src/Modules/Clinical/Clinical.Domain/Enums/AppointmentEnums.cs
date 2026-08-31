namespace Jacana.Clinical.Domain;

/// <summary>Lifecycle of a scheduled appointment.</summary>
public enum AppointmentStatus
{
    /// <summary>Booked, awaiting the visit.</summary>
    Scheduled,
    /// <summary>Visit started — a consultation is in progress.</summary>
    InProgress,
    /// <summary>Visit finished; the consultation was completed.</summary>
    Completed,
    /// <summary>Cancelled before the visit.</summary>
    Cancelled,
    /// <summary>The patient did not attend.</summary>
    NoShow
}

/// <summary>Purpose of the appointment — distinguishes follow-ups from new consults.</summary>
public enum AppointmentType
{
    Consultation,
    FollowUp,
    CheckUp,
    Review,
    Procedure,
    Other
}

/// <summary>Recurrence cadence for a series of appointments.</summary>
public enum RecurrencePattern
{
    None,
    Daily,
    Weekly,
    Monthly
}

/// <summary>How a consultation originated — powers the "tag" on the patient record.</summary>
public enum ConsultationSource
{
    Direct,
    Queue,
    Appointment
}

/// <summary>Lifecycle of a reception appointment request awaiting doctor approval.</summary>
public enum AppointmentRequestStatus
{
    Pending,
    Approved,
    Declined,
    Scheduled
}
