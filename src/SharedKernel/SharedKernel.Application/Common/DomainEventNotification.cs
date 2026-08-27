using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.SharedKernel.Application.Common;

/// <summary>
/// Bridges a BCL-only domain event into MediatR. The outbox dispatcher deserializes
/// a persisted <see cref="IDomainEvent"/> and publishes it wrapped in this notification
/// so handlers can subscribe as <c>INotificationHandler&lt;DomainEventNotification&lt;T&gt;&gt;</c>
/// without the Domain layer referencing MediatR.
/// </summary>
public interface IDomainEventNotification
{
    IDomainEvent DomainEvent { get; }
}

public sealed class DomainEventNotification<TDomainEvent> : INotification, IDomainEventNotification
    where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent) => DomainEvent = domainEvent;

    public TDomainEvent DomainEvent { get; }

    IDomainEvent IDomainEventNotification.DomainEvent => DomainEvent;
}
