using Jacana.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("notification_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Channel).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.RecipientPhoneOrEmail).HasMaxLength(256).IsRequired();
        builder.Property(m => m.TemplateCode).HasMaxLength(64).IsRequired();
        builder.Property(m => m.RenderedContent).HasMaxLength(2000).IsRequired();
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.AttemptCount).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(1000);

        builder.OwnsOne(m => m.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(m => m.RowVersion).IsRowVersion();
    }
}
