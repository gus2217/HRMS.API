using Jacana.Clinical.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Clinical.Infrastructure.Persistence.Configurations;

public sealed class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("consultations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PatientId).IsRequired();
        builder.Property(c => c.ClinicianUserId).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.StartedAtUtc).IsRequired();
        builder.Property(c => c.CompletedAtUtc);

        builder.OwnsOne(c => c.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.OwnsOne(c => c.Triage, t =>
        {
            t.Property(x => x.TemperatureCelsius).HasColumnName("TemperatureCelsius");
            t.Property(x => x.BloodPressure).HasColumnName("BloodPressure").HasMaxLength(16);
            t.Property(x => x.PulseRate).HasColumnName("PulseRate");
            t.Property(x => x.RespiratoryRate).HasColumnName("RespiratoryRate");
            t.Property(x => x.WeightKg).HasColumnName("WeightKg");
        });

        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasMany(c => c.Diagnoses).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Notes).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.LabOrders).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.PrescriptionOrders).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.ToTable("diagnoses");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.IcdCode).HasMaxLength(16).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500).IsRequired();
        builder.Property(d => d.IsPrimary).IsRequired();
    }
}

public sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.ToTable("clinical_notes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Content).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.AuthorUserId).IsRequired();
        builder.Property(n => n.RecordedAtUtc).IsRequired();
    }
}

public sealed class LabOrderReferenceConfiguration : IEntityTypeConfiguration<LabOrderReference>
{
    public void Configure(EntityTypeBuilder<LabOrderReference> builder)
    {
        builder.ToTable("lab_order_references");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.LabOrderId).IsRequired();
        builder.Property(r => r.StatusSnapshot).HasMaxLength(32).IsRequired();
    }
}

public sealed class PrescriptionOrderConfiguration : IEntityTypeConfiguration<PrescriptionOrder>
{
    public void Configure(EntityTypeBuilder<PrescriptionOrder> builder)
    {
        builder.ToTable("prescription_orders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PrescriptionId).IsRequired();
    }
}
