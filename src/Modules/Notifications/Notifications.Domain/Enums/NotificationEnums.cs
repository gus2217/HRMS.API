namespace Jacana.Notifications.Domain;

public enum NotificationChannel { Sms, WhatsApp, Email, InternalAlert }

public enum NotificationStatus { Pending, Sent, Failed, DeadLettered }
