using Jacana.SharedKernel.Domain;

namespace Jacana.Notifications.Domain;

/// <summary>
/// An in-app notification addressed to a specific user. Created by domain-event
/// handlers that fan out a clinical event to the right recipients (doctor, nurse,
/// pharmacist, lab tech). SMS/WhatsApp delivery will be layered on top later via
/// the outbound <see cref="NotificationMessage"/> outbox — this entity is the
/// read-side the recipient sees in the bell.
/// </summary>
public sealed class UserNotification : AggregateRoot<Guid>
{
    private UserNotification() { } // EF

    private UserNotification(
        Guid id,
        FacilityId facilityId,
        Guid recipientUserId,
        NotificationCategory category,
        string title,
        string message,
        string entityType,
        Guid? entityId,
        DateTime createdAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        RecipientUserId = recipientUserId;
        Category = category;
        Title = title;
        Message = message;
        EntityType = entityType;
        EntityId = entityId;
        CreatedAtUtc = createdAtUtc;
        IsRead = false;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid RecipientUserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    /// <summary>Deep-link target type, e.g. "Consultation", "LabOrder", "Prescription".</summary>
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Result<UserNotification> Create(
        FacilityId facilityId,
        Guid recipientUserId,
        NotificationCategory category,
        string title,
        string message,
        string entityType,
        Guid? entityId,
        DateTime createdAtUtc)
    {
        if (recipientUserId == Guid.Empty) return Error.Validation("Recipient is required.");
        if (string.IsNullOrWhiteSpace(title)) return Error.Validation("Title is required.");
        if (string.IsNullOrWhiteSpace(message)) return Error.Validation("Message is required.");

        return new UserNotification(
            Guid.NewGuid(), facilityId, recipientUserId, category,
            title.Trim(), message.Trim(), entityType, entityId, createdAtUtc);
    }

    public Result MarkRead(DateTime readAtUtc)
    {
        if (IsRead) return Result.Success();
        IsRead = true;
        ReadAtUtc = readAtUtc;
        return Result.Success();
    }
}
