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

        builder.ComplexProperty(c => c.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.OwnsOne(c => c.Triage, t =>
        {
            t.Property(x => x.TemperatureCelsius).HasColumnName("TemperatureCelsius");
            t.Property(x => x.BloodPressure).HasColumnName("BloodPressure").HasMaxLength(16);
            t.Property(x => x.PulseRate).HasColumnName("PulseRate");
            t.Property(x => x.RespiratoryRate).HasColumnName("RespiratoryRate");
            t.Property(x => x.WeightKg).HasColumnName("WeightKg");
        });

        builder.Property(c => c.RowVersion).IsConcurrencyToken();

        builder.HasMany(c => c.Diagnoses).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Notes).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.LabOrders).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.PrescriptionOrders).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Referrals).WithOne().HasForeignKey("ConsultationId").OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Documentation).WithOne()
            .HasForeignKey<ClinicalDocumentation>("ConsultationId")
            .OnDelete(DeleteBehavior.Cascade);
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

public sealed class ClinicalDocumentationConfiguration : IEntityTypeConfiguration<ClinicalDocumentation>
{
    public void Configure(EntityTypeBuilder<ClinicalDocumentation> builder)
    {
        builder.ToTable("clinical_documentations");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.ConsultationId).IsRequired();

        builder.Property(d => d.ChiefComplaint).HasMaxLength(2000);
        builder.Property(d => d.HistoryOfPresentingIllness).HasMaxLength(8000);

        builder.Property(d => d.PastMedicalHistory).HasMaxLength(4000);
        builder.Property(d => d.PastSurgicalHistory).HasMaxLength(4000);
        builder.Property(d => d.FamilyHistory).HasMaxLength(4000);
        builder.Property(d => d.SocialHistory).HasMaxLength(4000);
        builder.Property(d => d.GynaecologicalHistory).HasMaxLength(4000);
        builder.Property(d => d.ObstetricHistory).HasMaxLength(4000);
        builder.Property(d => d.DrugHistory).HasMaxLength(4000);

        builder.Property(d => d.RosGeneral).HasMaxLength(4000);
        builder.Property(d => d.RosCardiovascular).HasMaxLength(4000);
        builder.Property(d => d.RosRespiratory).HasMaxLength(4000);
        builder.Property(d => d.RosGastrointestinal).HasMaxLength(4000);
        builder.Property(d => d.RosGenitourinary).HasMaxLength(4000);
        builder.Property(d => d.RosMusculoskeletal).HasMaxLength(4000);
        builder.Property(d => d.RosNeurological).HasMaxLength(4000);
        builder.Property(d => d.RosDermatological).HasMaxLength(4000);
        builder.Property(d => d.RosEntEyes).HasMaxLength(4000);
        builder.Property(d => d.RosEndocrine).HasMaxLength(4000);

        builder.Property(d => d.ExamGeneralAppearance).HasMaxLength(4000);
        builder.Property(d => d.ExamHeadAndNeck).HasMaxLength(4000);
        builder.Property(d => d.ExamCardiovascular).HasMaxLength(4000);
        builder.Property(d => d.ExamRespiratory).HasMaxLength(4000);
        builder.Property(d => d.ExamAbdominal).HasMaxLength(4000);
        builder.Property(d => d.ExamGenitourinary).HasMaxLength(4000);
        builder.Property(d => d.ExamMusculoskeletal).HasMaxLength(4000);
        builder.Property(d => d.ExamNeurological).HasMaxLength(4000);
        builder.Property(d => d.ExamSkin).HasMaxLength(4000);
        builder.Property(d => d.ExamLymphatic).HasMaxLength(4000);

        builder.Property(d => d.LastSavedAtUtc);
        builder.Property(d => d.LastSavedByUserId);
    }
}

public sealed class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.ToTable("referrals");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ConsultationId).IsRequired();
        builder.Property(r => r.ReferredToFacility).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ReferredToUnit).HasMaxLength(200);
        builder.Property(r => r.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.Priority).HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.Notes).HasMaxLength(4000);
        builder.Property(r => r.ReferredByUserId).IsRequired();
        builder.Property(r => r.ReferredAtUtc).IsRequired();
    }
}
