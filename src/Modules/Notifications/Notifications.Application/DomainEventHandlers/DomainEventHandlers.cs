using Jacana.Clinical.Domain;
using Jacana.Inpatient.Domain;
using Jacana.Laboratory.Domain;
using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Application.DTOs;
using Jacana.Notifications.Domain;
using Jacana.Pharmacy.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Notifications.Application.DomainEventHandlers;

/// <summary>
/// Domain-event → in-app notification fan-out. Each handler resolves the recipient
/// user IDs (a specific clinician, or everyone in a role), filters them through the
/// per-user delivery preferences (defaults-on), creates a <see cref="UserNotification"/>
/// per recipient, commits through the shared unit-of-work set, then pushes the
/// committed notifications to online recipients over SignalR.
/// SMS/WhatsApp delivery is layered later on top of the same events via the
/// <see cref="NotificationMessage"/> outbox — the preference SmsEnabled flag is
/// already persisted and will gate that channel without event changes.
/// </summary>
public static class NotificationRoles
{
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";
    public const string Pharmacist = "Pharmacist";
    public const string LabTechnician = "LabTechnician";
    public const string Accountant = "Accountant";
    public const string Cashier = "Cashier";
}

/// <summary>Creates a notification, buffers it for push, and returns it.</summary>
internal static class NotificationFanout
{
    public static async Task CreateAsync(
        IUserNotificationRepository notifications,
        List<UserNotification> buffer,
        FacilityId facilityId,
        Guid recipientUserId,
        NotificationCategory category,
        string title,
        string message,
        string entityType,
        Guid? entityId,
        string? link,
        DateTime createdAtUtc,
        CancellationToken ct)
    {
        var n = UserNotification.Create(
            facilityId, recipientUserId, category, title, message, entityType, entityId, link, createdAtUtc);
        if (n.IsSuccess)
        {
            await notifications.AddAsync(n.Value, ct);
            buffer.Add(n.Value);
        }
    }
}

// ── Consultation / appointment requested → clinic doctors ──────────────────────

public sealed class ConsultationRequestedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<ConsultationRequestedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ConsultationRequestedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct),
            NotificationCategory.ConsultationRequested, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.ConsultationRequested,
                "Consultation requested",
                $"A patient has been queued for the {e.ClinicType} clinic.",
                "Queue", e.QueueEntryId, "/queue", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

public sealed class AppointmentRequestedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<AppointmentRequestedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AppointmentRequestedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor], ct),
            NotificationCategory.AppointmentRequested, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.AppointmentRequested,
                "Appointment booked",
                $"A new appointment has been booked for the {e.ClinicType} clinic.",
                "Appointment", e.AppointmentId, "/appointments", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

public sealed class AppointmentRequestRaisedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<AppointmentRequestRaisedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<AppointmentRequestRaisedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor], ct),
            NotificationCategory.AppointmentRequested, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.AppointmentRequested,
                "Appointment request",
                $"Reception has raised an appointment request for the {e.ClinicType} clinic — please review.",
                "AppointmentRequest", e.AppointmentRequestId, "/appointments", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

// ── Laboratory ──────────────────────────────────────────────────────────────────

/// <summary>Lab order placed → notify lab technicians to process it.</summary>
public sealed class LabOrderPlacedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<LabOrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabOrderCreatedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.LabTechnician], ct),
            NotificationCategory.LabResultReady, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.LabResultReady,
                "Lab order placed",
                $"A lab order ({e.Tests.Count} test(s)) has been placed for this patient.",
                "LabOrder", e.LabOrderId, "/lab", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

/// <summary>Lab order completed → notify the ordering clinician their results are in.</summary>
public sealed class LabOrderCompletedHandler(
    IUserNotificationRepository notifications,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<LabOrderCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabOrderCompletedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            [e.OrderedByUserId], NotificationCategory.LabResultReady, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.LabResultReady,
                "Lab results ready",
                "All tests on the lab order have been resulted — the results are ready for review.",
                "LabOrder", e.LabOrderId, $"/patients/{e.PatientId}", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

/// <summary>
/// Imaging/procedure report recorded → notify the ordering clinician the result
/// is ready for review (same fan-out as lab results).
/// </summary>
public sealed class DiagnosticOrderReportedHandler(
    IUserNotificationRepository notifications,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<DiagnosticOrderReportedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<DiagnosticOrderReportedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            [e.OrderedByUserId], NotificationCategory.DiagnosticResultReady, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.DiagnosticResultReady,
                "Imaging/procedure result ready",
                "The report for the ordered imaging/procedure is ready for review.",
                "DiagnosticOrder", e.DiagnosticOrderId, $"/patients/{e.PatientId}", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

// ── Pharmacy ────────────────────────────────────────────────────────────────────

/// <summary>Prescription initiated → notify pharmacists to prepare it.</summary>
public sealed class PrescriptionInitiatedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PrescriptionCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PrescriptionCreatedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Pharmacist], ct),
            NotificationCategory.PrescriptionInitiated, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PrescriptionInitiated,
                "Prescription to dispense",
                $"A new prescription ({e.Items.Count} item(s)) is awaiting dispensing.",
                "Prescription", e.PrescriptionId, "/pharmacy", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

// ── Inpatient ───────────────────────────────────────────────────────────────────

public sealed class PatientAdmittedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PatientAdmittedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientAdmittedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct),
            NotificationCategory.PatientAdmitted, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PatientAdmitted,
                "Patient admitted",
                $"A patient has been admitted to {e.WardName}.",
                "Admission", e.AdmissionId, "/wards", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

public sealed class PatientDischargedHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PatientDischargedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientDischargedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct),
            NotificationCategory.PatientDischarged, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PatientDischarged,
                "Patient discharged",
                "A patient has been discharged.",
                "Admission", e.AdmissionId, "/wards", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

public sealed class PatientTransferredHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<PatientTransferredDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PatientTransferredDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync([NotificationRoles.Doctor, NotificationRoles.Nurse], ct),
            NotificationCategory.PatientTransferred, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.PatientTransferred,
                "Patient transferred",
                $"A patient has been transferred from {e.FromWardName} to {e.ToWardName}.",
                "Admission", e.AdmissionId, "/wards", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

// ── Visit complete → billing desk (accountant / receptionist / cashier) ─────────

/// <summary>
/// Consultation completed → the visit is billed (auto-billing issues the invoice),
/// so the accounting/reception/cashier staff are alerted to collect/close the bill.
/// </summary>
public sealed class ConsultationCompletedBillingNotificationHandler(
    IUserNotificationRepository notifications,
    IUserRoleLookup roles,
    INotificationPreferenceRepository preferences,
    INotificationPusher pusher,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IClock clock)
    : INotificationHandler<DomainEventNotification<ConsultationCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ConsultationCompletedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var recipients = await preferences.FilterInAppEnabledAsync(
            await roles.GetUserIdsByRolesAsync(
                [NotificationRoles.Accountant, NotificationRoles.Receptionist, NotificationRoles.Cashier], ct),
            NotificationCategory.InvoiceIssued, ct);

        var created = new List<UserNotification>();
        foreach (var userId in recipients)
        {
            await NotificationFanout.CreateAsync(notifications, created,
                FacilityId.From(e.FacilityId), userId, NotificationCategory.InvoiceIssued,
                "Bill ready — collect payment",
                "A consultation has been completed and the visit invoice issued. Collect payment at billing.",
                "Consultation", e.ConsultationId, "/billing", clock.UtcNow, ct);
        }
        await NotificationCommit.CommitAndPushAsync(unitsOfWork, pusher, created, ct);
    }
}

internal static class NotificationCommit
{
    /// <summary>
    /// Commits every unit of work with tracked changes, then pushes the committed
    /// notifications to online recipients. Mirrors the Billing AutoBilling pattern —
    /// each module registers its own IUnitOfWork, so a plain single-IUnitOfWork
    /// injection would resolve the wrong context.
    /// </summary>
    public static async Task CommitAndPushAsync(
        IEnumerable<IUnitOfWork> unitsOfWork,
        INotificationPusher pusher,
        IReadOnlyList<UserNotification> created,
        CancellationToken ct)
    {
        var tasks = unitsOfWork
            .Where(u => u.HasChanges)
            .Select(u => u.SaveChangesAsync(ct));
        await Task.WhenAll(tasks);

        foreach (var n in created)
        {
            var dto = new UserNotificationDto(
                n.Id, n.Category.ToString(), n.Title, n.Message,
                n.EntityType, n.EntityId, n.Link, n.IsRead, n.CreatedAtUtc);
            await pusher.PushAsync(n.RecipientUserId, dto, ct);
        }
    }
}
