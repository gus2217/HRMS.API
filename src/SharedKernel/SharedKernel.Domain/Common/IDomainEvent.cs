namespace Jacana.SharedKernel.Domain;

/// <summary>
/// A domain event. BCL-only by design (the Definition-of-Done rule "Domain has zero
/// references beyond SharedKernel.Domain + BCL" is stricter than the INotification
/// note in the spec); the MediatR bridge lives in SharedKernel.Application.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
