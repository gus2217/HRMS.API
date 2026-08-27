namespace Jacana.Clinical.Domain;

/// <summary>
/// Thrown when an aggregate is advanced to a status that is not a legal transition
/// from its current status. This is an exceptional (programmer/race) condition —
/// expected business-rule rejections return Result failures instead.
/// </summary>
public sealed class InvalidConsultationTransitionException : InvalidOperationException
{
    public ConsultationStatus From { get; }
    public ConsultationStatus To { get; }

    public InvalidConsultationTransitionException(ConsultationStatus from, ConsultationStatus to)
        : base($"Illegal consultation transition: {from} -> {to}.")
    {
        From = from;
        To = to;
    }
}
