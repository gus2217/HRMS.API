using Jacana.SharedKernel.Domain;

namespace Jacana.Notifications.Domain;

/// <summary>
/// A notification message. Backs the outbox pattern for outbound notifications —
/// dispatched exclusively by the Hangfire background processor, never inline in a
/// request handler.
/// </summary>
public sealed class NotificationMessage : AggregateRoot<Guid>
{
    private NotificationMessage() { } // EF

    private NotificationMessage(Guid id, FacilityId facilityId, NotificationChannel channel,
        string recipientPhoneOrEmail, string templateCode, string renderedContent)
        : base(id)
    {
        FacilityId = facilityId;
        Channel = channel;
        RecipientPhoneOrEmail = recipientPhoneOrEmail;
        TemplateCode = templateCode;
        RenderedContent = renderedContent;
        Status = NotificationStatus.Pending;
        AttemptCount = 0;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public NotificationChannel Channel { get; private set; }
    public string RecipientPhoneOrEmail { get; private set; } = string.Empty;
    public string TemplateCode { get; private set; } = string.Empty;
    public string RenderedContent { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }

    public static Result<NotificationMessage> Create(
        Guid id, FacilityId facilityId, NotificationChannel channel,
        string recipientPhoneOrEmail, string templateCode, string renderedContent)
    {
        if (string.IsNullOrWhiteSpace(recipientPhoneOrEmail))
            return Error.Validation("Recipient is required.");
        if (string.IsNullOrWhiteSpace(templateCode))
            return Error.Validation("Template code is required.");
        if (string.IsNullOrWhiteSpace(renderedContent))
            return Error.Validation("Rendered content is required.");
        return new NotificationMessage(id, facilityId, channel,
            recipientPhoneOrEmail.Trim(), templateCode.Trim(), renderedContent.Trim());
    }

    public void MarkSent() { Status = NotificationStatus.Sent; }

    public void RecordFailure(string error)
    {
        AttemptCount++;
        LastError = error;
        Status = AttemptCount >= 5 ? NotificationStatus.DeadLettered : NotificationStatus.Failed;
    }
}
