using Jacana.Notifications.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Notifications;

public class NotificationMessageTests
{
    [Fact]
    public void Create_requires_recipient_template_and_content()
    {
        var facility = FacilityId.New();
        Assert.True(NotificationMessage.Create(Guid.NewGuid(), facility, NotificationChannel.Sms, "", "T", "content").IsFailure);
        Assert.True(NotificationMessage.Create(Guid.NewGuid(), facility, NotificationChannel.Sms, "r", "", "content").IsFailure);
        Assert.True(NotificationMessage.Create(Guid.NewGuid(), facility, NotificationChannel.Sms, "r", "T", "").IsFailure);
    }

    [Fact]
    public void RecordFailure_deadletters_after_five_attempts()
    {
        var message = NotificationMessage.Create(Guid.NewGuid(), FacilityId.New(),
            NotificationChannel.Sms, "+254712345678", "TPL", "Hello").Value;

        for (var i = 0; i < 5; i++) message.RecordFailure("boom");

        Assert.Equal(NotificationStatus.DeadLettered, message.Status);
        Assert.Equal(5, message.AttemptCount);
    }
}
