namespace Jacana.Notifications.Domain;

/// <summary>Category of an in-app notification (drives the bell badge/icon).</summary>
public enum NotificationCategory
{
    ConsultationRequested,
    AppointmentRequested,
    LabResultReady,
    DiagnosticResultReady,
    PrescriptionInitiated,
    PatientAdmitted,
    PatientDischarged,
    PatientTransferred,
    ReferralCreated,
    InvoiceIssued,
    System
}
