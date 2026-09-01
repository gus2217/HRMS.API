using Jacana.SharedKernel.Domain;

namespace Jacana.Notifications.Domain;

/// <summary>
/// Per-user, per-category delivery preference. Missing rows mean "enabled"
/// (defaults-on). The SMS flag is respected the moment an SMS channel is
/// implemented — the fan-out handlers already filter on it, so no event
/// wiring needs to change later.
/// </summary>
public sealed class NotificationPreference : Entity<Guid>
{
    private NotificationPreference() { } // EF

    private NotificationPreference(
        Guid id,
        FacilityId facilityId,
        Guid recipientUserId,
        NotificationCategory category,
        bool inAppEnabled,
        bool smsEnabled,
        DateTime updatedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        RecipientUserId = recipientUserId;
        Category = category;
        InAppEnabled = inAppEnabled;
        SmsEnabled = smsEnabled;
        UpdatedAtUtc = updatedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid RecipientUserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool SmsEnabled { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Result<NotificationPreference> Create(
        FacilityId facilityId,
        Guid recipientUserId,
        NotificationCategory category,
        bool inAppEnabled,
        bool smsEnabled,
        DateTime updatedAtUtc)
    {
        if (recipientUserId == Guid.Empty) return Error.Validation("Recipient is required.");
        return new NotificationPreference(
            Guid.NewGuid(), facilityId, recipientUserId, category, inAppEnabled, smsEnabled, updatedAtUtc);
    }

    public Result Update(bool inAppEnabled, bool smsEnabled, DateTime updatedAtUtc)
    {
        InAppEnabled = inAppEnabled;
        SmsEnabled = smsEnabled;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }
}
