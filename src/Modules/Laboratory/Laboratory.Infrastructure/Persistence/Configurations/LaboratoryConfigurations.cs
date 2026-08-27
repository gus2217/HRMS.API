using Jacana.Laboratory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Laboratory.Infrastructure.Persistence.Configurations;

public sealed class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.ToTable("lab_orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.PatientId).IsRequired();
        builder.Property(o => o.ConsultationId).IsRequired();
        builder.Property(o => o.OrderedByUserId).IsRequired();
        builder.Property(o => o.OrderedAtUtc).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(32);

        builder.ComplexProperty(o => o.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(o => o.RowVersion).IsConcurrencyToken();

        builder.HasMany(o => o.Tests).WithOne().HasForeignKey("LabOrderId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class LabTestItemConfiguration : IEntityTypeConfiguration<LabTestItem>
{
    public void Configure(EntityTypeBuilder<LabTestItem> builder)
    {
        builder.ToTable("lab_test_items");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TestCode).HasMaxLength(32).IsRequired();
        builder.Property(t => t.TestName).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(t => t.ResultValue).HasMaxLength(100);
        builder.Property(t => t.ResultUnit).HasMaxLength(32);
        builder.Property(t => t.ReferenceRange).HasMaxLength(100);
        builder.Property(t => t.IsAbnormal);
        builder.Property(t => t.ResultedByUserId);
        builder.Property(t => t.ResultedAtUtc);
    }
}
