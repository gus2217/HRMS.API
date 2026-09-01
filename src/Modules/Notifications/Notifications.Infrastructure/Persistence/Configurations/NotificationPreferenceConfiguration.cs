using Jacana.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RecipientUserId).IsRequired();
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(48);
        builder.Property(p => p.InAppEnabled).IsRequired();
        builder.Property(p => p.SmsEnabled).IsRequired();
        builder.Property(p => p.UpdatedAtUtc).IsRequired();

        builder.ComplexProperty(p => p.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());

        // One preference row per user per category.
        builder.HasIndex(p => new { p.RecipientUserId, p.Category }).IsUnique();
    }
}
