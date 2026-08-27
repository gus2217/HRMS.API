using Jacana.Inpatient.Domain;
using Jacana.Laboratory.Domain;
using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Domain;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Notifications.Application.DomainEventHandlers;

/// <summary>
/// Consumes <see cref="LabResultRecordedDomainEvent"/> (published by the Laboratory
/// module via the outbox) and queues an internal alert for the ordering clinician.
/// Cross-module coupling is one-directional via the domain event — never a direct call.
/// </summary>
public sealed class LabResultRecordedHandler(INotificationMessageRepository messages)
    : INotificationHandler<DomainEventNotification<LabResultRecordedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabResultRecordedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var message = NotificationMessage.Create(
            Guid.NewGuid(),
            FacilityId.New(), // facility resolved from lookup in production; event carries patient context
            NotificationChannel.InternalAlert,
            "internal",
            "LabResultReady",
            $"Lab result recorded for patient {e.PatientId} (order {e.LabOrderId}, test {e.TestItemId}).");

        if (message.IsSuccess)
            await messages.AddAsync(message.Value, ct);
    }
}

public sealed class PatientAdmittedHandler(INotificationMessageRepository messages)
    : INotificationHandler<DomainEventNotification<PatientAdmittedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientAdmittedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var message = NotificationMessage.Create(
            Guid.NewGuid(), FacilityId.New(), NotificationChannel.InternalAlert, "internal",
            "PatientAdmitted", $"Patient {e.PatientId} admitted to {e.WardName}.");

        if (message.IsSuccess)
            await messages.AddAsync(message.Value, ct);
    }
}

public sealed class PatientDischargedHandler(INotificationMessageRepository messages)
    : INotificationHandler<DomainEventNotification<PatientDischargedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientDischargedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var message = NotificationMessage.Create(
            Guid.NewGuid(), FacilityId.New(), NotificationChannel.InternalAlert, "internal",
            "PatientDischarged", $"Patient {e.PatientId} discharged.");

        if (message.IsSuccess)
            await messages.AddAsync(message.Value, ct);
    }
}
