namespace Jacana.Clinical.Domain;

/// <summary>Kind of a diagnostic order.</summary>
public enum DiagnosticOrderType
{
    Imaging,
    Procedure
}

/// <summary>Lifecycle of a diagnostic order.</summary>
public enum DiagnosticOrderStatus
{
    Ordered,
    Scheduled,
    Performed,
    Reported,
    Cancelled
}

/// <summary>Clinical urgency of a diagnostic order.</summary>
public enum DiagnosticOrderPriority
{
    Routine,
    Urgent,
    Emergency
}
