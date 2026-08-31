namespace Jacana.Identity.Application;

/// <summary>
/// Canonical permission codes. Handlers and policies reference these constants —
/// never raw string literals. Values follow {Module}.{Action} convention.
/// </summary>
public static class Permissions
{
    public static class Users
    {
        public const string View = "Identity.User.View";
        public const string Register = "Identity.User.Register";
        public const string AssignRole = "Identity.User.AssignRole";
        public const string Suspend = "Identity.User.Suspend";
    }

    public static class Roles
    {
        public const string View = "Identity.Role.View";
        public const string Manage = "Identity.Role.Manage";
    }

    public static class Patients
    {
        public const string Register = "Patient.Register";
        public const string View = "Patient.View";
        public const string Update = "Patient.Update";

        /// <summary>
        /// Grants access to confidential patient data (phone, SHA number, address,
        /// next-of-kin details). Roles that only need to identify a patient
        /// (e.g. Lab Technician, Pharmacist) hold Patient.View without this.
        /// </summary>
        public const string ConfidentialView = "Patient.ConfidentialView";
    }

    public static class Billing
    {
        public const string IssueInvoice = "Billing.IssueInvoice";
        public const string RecordPayment = "Billing.RecordPayment";
        public const string View = "Billing.View";
    }

    public static class Clinical
    {
        public const string Consult = "Clinical.Consult";
        public const string RecordDiagnosis = "Clinical.RecordDiagnosis";
        public const string View = "Clinical.View";
    }

    public static class Queue
    {
        /// <summary>Reception queues patients for consultations.</summary>
        public const string Create = "Queue.Create";
        /// <summary>View the consultation queue board.</summary>
        public const string View = "Queue.View";
        /// <summary>Accept queue entries and register the consultation.</summary>
        public const string Accept = "Queue.Accept";
    }

    public static class Appointment
    {
        /// <summary>Book/start/manage appointments (clinicians).</summary>
        public const string Create = "Appointment.Create";
        /// <summary>View the appointment calendar, day queue and requests.</summary>
        public const string View = "Appointment.View";
        /// <summary>Reception raises appointment requests for a clinic.</summary>
        public const string Request = "Appointment.Request";
        /// <summary>Approve or decline appointment requests.</summary>
        public const string Approve = "Appointment.Approve";
    }

    public static class Lab
    {
        public const string Order = "Laboratory.Order";
        public const string RecordResult = "Laboratory.RecordResult";
    }

    public static class Pharmacy
    {
        public const string Dispense = "Pharmacy.Dispense";
    }

    public static class Inventory
    {
        public const string Receive = "Inventory.Receive";
        public const string Adjust = "Inventory.Adjust";
    }
}
