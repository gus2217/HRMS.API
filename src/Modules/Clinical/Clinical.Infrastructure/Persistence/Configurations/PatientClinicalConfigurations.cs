using Jacana.Clinical.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Clinical.Infrastructure.Persistence.Configurations;

public sealed class VitalSignConfiguration : IEntityTypeConfiguration<VitalSign>
{
    public void Configure(EntityTypeBuilder<VitalSign> builder)
    {
        builder.ToTable("vital_signs");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.PatientId).IsRequired();
        builder.Property(v => v.TemperatureCelsius);
        builder.Property(v => v.SystolicBp);
        builder.Property(v => v.DiastolicBp);
        builder.Property(v => v.PulseRate);
        builder.Property(v => v.RespiratoryRate);
        builder.Property(v => v.OxygenSaturation);
        builder.Property(v => v.WeightKg);
        builder.Property(v => v.HeightCm);
        builder.Property(v => v.Bmi);
        builder.Property(v => v.RecordedByUserId).IsRequired();
        builder.Property(v => v.RecordedAtUtc).IsRequired();

        builder.ComplexProperty(v => v.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(v => v.RowVersion).IsConcurrencyToken();

        // Trend reads filter by patient and order by recorded time.
        builder.HasIndex(v => new { v.PatientId, v.RecordedAtUtc });
    }
}

public sealed class ImmunizationConfiguration : IEntityTypeConfiguration<Immunization>
{
    public void Configure(EntityTypeBuilder<Immunization> builder)
    {
        builder.ToTable("immunizations");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.PatientId).IsRequired();
        builder.Property(i => i.VaccineName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.DoseNumber).IsRequired();
        builder.Property(i => i.AdministeredDate).IsRequired();
        builder.Property(i => i.NextDueDate);
        builder.Property(i => i.LotNumber).HasMaxLength(100);
        builder.Property(i => i.Site).HasMaxLength(100);
        builder.Property(i => i.Notes).HasMaxLength(500);
        builder.Property(i => i.RecordedByUserId).IsRequired();
        builder.Property(i => i.RecordedAtUtc).IsRequired();

        builder.ComplexProperty(i => i.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(i => i.RowVersion).IsConcurrencyToken();

        builder.HasIndex(i => new { i.PatientId, i.AdministeredDate });
    }
}

public sealed class ConditionConfiguration : IEntityTypeConfiguration<Condition>
{
    public void Configure(EntityTypeBuilder<Condition> builder)
    {
        builder.ToTable("conditions");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PatientId).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(16);
        builder.Property(c => c.Description).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(c => c.OnsetDate).IsRequired();
        builder.Property(c => c.ResolvedDate);
        builder.Property(c => c.RecordedByUserId).IsRequired();
        builder.Property(c => c.RecordedAtUtc).IsRequired();

        builder.ComplexProperty(c => c.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(c => c.RowVersion).IsConcurrencyToken();

        // Problem-list reads filter by patient + active status.
        builder.HasIndex(c => new { c.PatientId, c.Status });
    }
}
