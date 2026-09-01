using Jacana.Inpatient.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Inpatient.Infrastructure.Persistence.Configurations;

public sealed class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.ToTable("wards");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).HasMaxLength(100).IsRequired();
        builder.Property(w => w.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(w => w.TotalBeds).IsRequired();
        builder.Property(w => w.IsActive).IsRequired();

        builder.ComplexProperty(w => w.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(w => w.RowVersion).IsConcurrencyToken();

        builder.HasIndex(w => w.IsActive);
    }
}

public sealed class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> builder)
    {
        builder.ToTable("admissions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.AdmittingClinicianUserId).IsRequired();
        builder.Property(a => a.WardId).IsRequired();
        builder.Property(a => a.WardName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.BedNumber).HasMaxLength(50).IsRequired();
        builder.Property(a => a.AdmittingDiagnosis).HasMaxLength(2000);
        builder.Property(a => a.AttendingClinicianUserId);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.AdmittedAtUtc).IsRequired();
        builder.Property(a => a.DischargedAtUtc);

        builder.ComplexProperty(a => a.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(a => a.RowVersion).IsConcurrencyToken();

        builder.HasMany(a => a.Notes).WithOne().HasForeignKey("AdmissionId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(a => a.MedicalRecords).WithOne().HasForeignKey("AdmissionId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WardNoteConfiguration : IEntityTypeConfiguration<WardNote>
{
    public void Configure(EntityTypeBuilder<WardNote> builder)
    {
        builder.ToTable("ward_notes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Content).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.AuthorUserId).IsRequired();
        builder.Property(n => n.RecordedAtUtc).IsRequired();
    }
}

public sealed class WardMedicalRecordConfiguration : IEntityTypeConfiguration<WardMedicalRecord>
{
    public void Configure(EntityTypeBuilder<WardMedicalRecord> builder)
    {
        builder.ToTable("ward_medical_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AdmissionId).IsRequired();
        builder.Property(r => r.RecordedByUserId).IsRequired();
        builder.Property(r => r.RecordedAtUtc).IsRequired();

        builder.Property(r => r.TemperatureCelsius);
        builder.Property(r => r.SystolicBp);
        builder.Property(r => r.DiastolicBp);
        builder.Property(r => r.PulseRate);
        builder.Property(r => r.RespiratoryRate);
        builder.Property(r => r.OxygenSaturation);
        builder.Property(r => r.WeightKg);

        builder.Property(r => r.Subjective).HasMaxLength(8000);
        builder.Property(r => r.Objective).HasMaxLength(8000);
        builder.Property(r => r.Assessment).HasMaxLength(8000);
        builder.Property(r => r.Plan).HasMaxLength(8000);

        builder.HasMany(r => r.Attachments).WithOne().HasForeignKey("WardMedicalRecordId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WardRecordAttachmentConfiguration : IEntityTypeConfiguration<WardRecordAttachment>
{
    public void Configure(EntityTypeBuilder<WardRecordAttachment> builder)
    {
        builder.ToTable("ward_record_attachments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.WardMedicalRecordId).IsRequired();
        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(a => a.SizeBytes).IsRequired();
        builder.Property(a => a.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(a => a.UploadedByUserId).IsRequired();
        builder.Property(a => a.UploadedAtUtc).IsRequired();
    }
}
