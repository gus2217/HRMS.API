using Jacana.Clinical.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Clinical.Infrastructure.Persistence.Configurations;

public sealed class PatientFlagConfiguration : IEntityTypeConfiguration<PatientFlag>
{
    public void Configure(EntityTypeBuilder<PatientFlag> builder)
    {
        builder.ToTable("patient_flags");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.PatientId).IsRequired();
        builder.Property(f => f.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(f => f.Message).HasMaxLength(500).IsRequired();
        builder.Property(f => f.IsActive).IsRequired();
        builder.Property(f => f.CreatedByUserId).IsRequired();
        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.Property(f => f.DeactivatedByUserId);
        builder.Property(f => f.DeactivatedAtUtc);

        builder.ComplexProperty(f => f.FacilityId, c => c.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(f => f.RowVersion).IsConcurrencyToken();

        // Banner reads filter active flags by patient.
        builder.HasIndex(f => new { f.PatientId, f.IsActive });
    }
}

public sealed class PatientAttachmentConfiguration : IEntityTypeConfiguration<PatientAttachment>
{
    public void Configure(EntityTypeBuilder<PatientAttachment> builder)
    {
        builder.ToTable("patient_attachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(a => a.SizeBytes).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(64);
        builder.Property(a => a.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(a => a.UploadedByUserId).IsRequired();
        builder.Property(a => a.UploadedAtUtc).IsRequired();

        builder.ComplexProperty(a => a.FacilityId, c => c.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(a => a.RowVersion).IsConcurrencyToken();

        builder.HasIndex(a => new { a.PatientId, a.UploadedAtUtc });
    }
}

public sealed class DiagnosticOrderConfiguration : IEntityTypeConfiguration<DiagnosticOrder>
{
    public void Configure(EntityTypeBuilder<DiagnosticOrder> builder)
    {
        builder.ToTable("diagnostic_orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.PatientId).IsRequired();
        builder.Property(o => o.ConsultationId);
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.BodySite).HasMaxLength(100);
        builder.Property(o => o.ClinicalIndication).HasMaxLength(2000).IsRequired();
        builder.Property(o => o.Priority).HasConversion<string>().HasMaxLength(16);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(o => o.OrderedByUserId).IsRequired();
        builder.Property(o => o.OrderedAtUtc).IsRequired();
        builder.Property(o => o.Report).HasMaxLength(8000);
        builder.Property(o => o.ReportedByUserId);
        builder.Property(o => o.ReportedAtUtc);

        builder.ComplexProperty(o => o.FacilityId, c => c.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(o => o.RowVersion).IsConcurrencyToken();

        builder.HasIndex(o => new { o.PatientId, o.Status });
        builder.HasIndex(o => o.ConsultationId);
    }
}
