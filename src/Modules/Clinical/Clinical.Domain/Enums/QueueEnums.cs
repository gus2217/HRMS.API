namespace Jacana.Clinical.Domain;

/// <summary>
/// Triage priority set by reception when queueing a patient for a consultation.
/// Mirrors the referral priority ladder so urgent cases surface first.
/// </summary>
public enum QueuePriority
{
    Routine,
    Urgent,
    Emergency
}

/// <summary>Lifecycle of a consultation queue entry.</summary>
public enum QueueStatus
{
    /// <summary>Queued by reception; awaiting a clinician.</summary>
    Waiting,
    /// <summary>Accepted by a clinician — a consultation was registered.</summary>
    Accepted,
    /// <summary>The linked consultation was completed.</summary>
    Completed,
    /// <summary>Cancelled by reception (patient left, duplicate, etc.).</summary>
    Cancelled
}
