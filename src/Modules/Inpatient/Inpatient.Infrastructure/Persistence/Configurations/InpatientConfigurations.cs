using Jacana.Inpatient.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Inpatient.Infrastructure.Persistence.Configurations;

public sealed class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> builder)
    {
        builder.ToTable("admissions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.AdmittingClinicianUserId).IsRequired();
        builder.Property(a => a.WardName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.BedNumber).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.AdmittedAtUtc).IsRequired();
        builder.Property(a => a.DischargedAtUtc);

        builder.OwnsOne(a => a.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasMany(a => a.Notes).WithOne().HasForeignKey("AdmissionId").OnDelete(DeleteBehavior.Cascade);
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
