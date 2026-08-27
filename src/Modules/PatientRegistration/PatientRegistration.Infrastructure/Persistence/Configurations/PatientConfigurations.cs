using Jacana.PatientRegistration.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.PatientRegistration.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PatientNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(p => p.PatientNumber).IsUnique();

        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.DateOfBirth).IsRequired();
        builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.MaritalStatus).HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(16);

        builder.ComplexProperty(p => p.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.ComplexProperty(p => p.Phone, ph => ph.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired());
        builder.ComplexProperty(p => p.Address, a =>
        {
            a.Property(x => x.County).HasColumnName("County").HasMaxLength(100).IsRequired();
            a.Property(x => x.SubCounty).HasColumnName("SubCounty").HasMaxLength(100);
            a.Property(x => x.Ward).HasColumnName("Ward").HasMaxLength(100);
            a.Property(x => x.Line1).HasColumnName("AddressLine1").HasMaxLength(200);
        });

        builder.Property(p => p.ShaNumber).HasMaxLength(64);

        builder.Property(p => p.RowVersion).IsConcurrencyToken();

        builder.HasMany(p => p.NextOfKin).WithOne().HasForeignKey("PatientId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Allergies).WithOne().HasForeignKey("PatientId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Consents).WithOne().HasForeignKey("PatientId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NextOfKinConfiguration : IEntityTypeConfiguration<NextOfKin>
{
    public void Configure(EntityTypeBuilder<NextOfKin> builder)
    {
        builder.ToTable("next_of_kin");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.FullName).HasMaxLength(150).IsRequired();
        builder.Property(k => k.Relationship).HasMaxLength(50).IsRequired();
        builder.ComplexProperty(k => k.Phone, p => p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired());
    }
}

public sealed class AllergyConfiguration : IEntityTypeConfiguration<Allergy>
{
    public void Configure(EntityTypeBuilder<Allergy> builder)
    {
        builder.ToTable("allergies");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Substance).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Notes).HasMaxLength(500);
    }
}

public sealed class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("consent_records");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.Granted).IsRequired();
        builder.Property(c => c.RecordedByUserId).IsRequired();
        builder.Property(c => c.RecordedAtUtc).IsRequired();
    }
}
