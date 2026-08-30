namespace Jacana.Clinical.Domain;

/// <summary>Clinical priority of a referral.</summary>
public enum ReferralPriority
{
    Routine,
    Urgent,
    Emergency
}

/// <summary>Lifecycle of a referral.</summary>
public enum ReferralStatus
{
    Pending,
    Accepted,
    Completed,
    Declined
}
