using Jacana.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Notifications.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("user_notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientUserId).IsRequired();
        builder.Property(n => n.Category).HasConversion<string>().HasMaxLength(48);
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(2000).IsRequired();
        builder.Property(n => n.EntityType).HasMaxLength(64);
        builder.Property(n => n.EntityId);
        builder.Property(n => n.IsRead).IsRequired();
        builder.Property(n => n.ReadAtUtc);
        builder.Property(n => n.CreatedAtUtc).IsRequired();

        builder.ComplexProperty(n => n.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(n => n.RowVersion).IsConcurrencyToken();

        // Bell reads: current user's notifications, unread first, newest first.
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead, n.CreatedAtUtc });
    }
}
