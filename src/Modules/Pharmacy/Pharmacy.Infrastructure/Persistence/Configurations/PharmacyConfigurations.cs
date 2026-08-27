using Jacana.Pharmacy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Pharmacy.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PatientId).IsRequired();
        builder.Property(p => p.ConsultationId).IsRequired();
        builder.Property(p => p.PrescribedByUserId).IsRequired();
        builder.Property(p => p.PrescribedAtUtc).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);

        builder.OwnsOne(p => p.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasMany(p => p.Items).WithOne().HasForeignKey("PrescriptionId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("prescription_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.DrugId).IsRequired();
        builder.Property(i => i.DosageInstructions).HasMaxLength(500).IsRequired();
        builder.Property(i => i.QuantityPrescribed).IsRequired();
        builder.Property(i => i.QuantityDispensed).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(32);
    }
}

public sealed class DispenseRecordConfiguration : IEntityTypeConfiguration<DispenseRecord>
{
    public void Configure(EntityTypeBuilder<DispenseRecord> builder)
    {
        builder.ToTable("dispense_records");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.PrescriptionItemId).IsRequired();
        builder.Property(d => d.QuantityDispensed).IsRequired();
        builder.Property(d => d.DispensedByUserId).IsRequired();
        builder.Property(d => d.DispensedAtUtc).IsRequired();

        builder.OwnsOne(d => d.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(d => d.RowVersion).IsRowVersion();
    }
}
