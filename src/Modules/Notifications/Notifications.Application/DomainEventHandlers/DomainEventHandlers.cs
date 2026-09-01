using Jacana.Clinical.Domain;
using Jacana.Inpatient.Domain;
using Jacana.Laboratory.Domain;
using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Domain;
using Jacana.Pharmacy.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Notifications.Application.DomainEventHandlers;

/// <summary>
/// Domain-event → in-app notification fan-out. Each handler resolves the recipient
/// user IDs (a specific clinician, or everyone in a role) and creates a
/// <see cref="UserNotification"/> per recipient, then commits through the shared
/// unit-of-work set — the same pattern the Billing auto-billing handlers use.
/// SMS/WhatsApp delivery is layered later on top of the same events via the
/// <see cref="NotificationMessage"/> outbox.
/// </summary>
public static class NotificationRoles
{
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Pharmacist = "Pharmacist";
    public const string LabTechnician = "LabTechnician";
}

// ── Consultation / appointment requested → clinic doctors ──────────────────────

public sealed class ConsultationRequestedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<ConsultationRequestedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ConsultationRequestedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.ConsultationRequested,
                "Consultation requested",
                $"A patient has been queued for the {e.ClinicType} clinic.",
                "Queue", e.QueueEntryId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

public sealed class AppointmentRequestedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<AppointmentRequestedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AppointmentRequestedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.AppointmentRequested,
                "Appointment booked",
                $"A new appointment has been booked for the {e.ClinicType} clinic.",
                "Appointment", e.AppointmentId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

public sealed class AppointmentRequestRaisedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<AppointmentRequestRaisedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AppointmentRequestRaisedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.AppointmentRequested,
                "Appointment request",
                $"Reception has raised an appointment request for the {e.ClinicType} clinic — please review.",
                "AppointmentRequest", e.AppointmentRequestId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

// ── Laboratory ──────────────────────────────────────────────────────────────────

/// <summary>Lab order placed → notify lab technicians to process it.</summary>
public sealed class LabOrderPlacedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<LabOrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabOrderCreatedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.LabTechnician], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.LabResultReady,
                "Lab order placed",
                $"A lab order ({e.Tests.Count} test(s)) has been placed for this patient.",
                "LabOrder", e.LabOrderId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

/// <summary>Lab order completed → notify the ordering clinician their results are in.</summary>
public sealed class LabOrderCompletedHandler(
    IUserNotificationRepository notifications,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<LabOrderCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabOrderCompletedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var n = UserNotification.Create(
            FacilityId.From(e.FacilityId), e.OrderedByUserId, NotificationCategory.LabResultReady,
            "Lab results ready",
            "All tests on the lab order have been resulted — the results are ready for review.",
            "LabOrder", e.LabOrderId, clock.UtcNow);
        if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

// ── Pharmacy ────────────────────────────────────────────────────────────────────

/// <summary>Prescription initiated → notify pharmacists to prepare it.</summary>
public sealed class PrescriptionInitiatedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PrescriptionCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PrescriptionCreatedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.Pharmacist], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PrescriptionInitiated,
                "Prescription to dispense",
                $"A new prescription ({e.Items.Count} item(s)) is awaiting dispensing.",
                "Prescription", e.PrescriptionId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

// ── Inpatient ───────────────────────────────────────────────────────────────────

public sealed class PatientAdmittedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PatientAdmittedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientAdmittedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PatientAdmitted,
                "Patient admitted",
                $"A patient has been admitted to {e.WardName}.",
                "Admission", e.AdmissionId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

public sealed class PatientDischargedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PatientDischargedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientDischargedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct);

        foreach (var userId in recipients)
        {
            var n = UserNotification.Create(
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PatientDischarged,
                "Patient discharged",
                "A patient has been discharged.",
                "Admission", e.AdmissionId, clock.UtcNow);
            if (n.IsSuccess) await notifications.AddAsync(n.Value, ct);
        }
        await NotificationCommit.CommitAsync(unitsOfWork, ct);
    }
}

internal static class NotificationCommit
{
    /// <summary>
    /// Commits every unit of work with tracked changes. Mirrors the Billing
    /// AutoBilling pattern — each module registers its own IUnitOfWork, so a
    /// plain single-IUnitOfWork injection would resolve the wrong context.
    /// </summary>
    public static Task CommitAsync(IEnumerable<IUnitOfWork> unitsOfWork, CancellationToken ct)
    {
        var tasks = unitsOfWork
            .Where(u => u.HasChanges)
            .Select(u => u.SaveChangesAsync(ct));
        return Task.WhenAll(tasks);
    }
}
