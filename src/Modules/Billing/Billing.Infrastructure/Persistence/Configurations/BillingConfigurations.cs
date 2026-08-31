using Jacana.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Billing.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.PatientId).IsRequired();
        builder.Property(i => i.ConsultationId);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(i => i.PrimaryPaymentMethod).HasConversion<string>().HasMaxLength(32);

        builder.ComplexProperty(i => i.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(i => i.RowVersion).IsConcurrencyToken();

        // TotalAmount is a computed getter-only property (sum of lines); it is not
        // persisted. For query performance in production, add a shadow column and
        // populate it via a SaveChanges interceptor.
        builder.Ignore(i => i.TotalAmount);

        builder.HasMany(i => i.Lines).WithOne().HasForeignKey("InvoiceId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ServiceCode).HasMaxLength(64).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(300).IsRequired();
        builder.Property(l => l.Quantity).IsRequired();
        builder.Property(l => l.SourceType).HasMaxLength(32);
        builder.Property(l => l.SourceReferenceId);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(16);
        builder.ComplexProperty(l => l.UnitPrice, m =>
        {
            m.Property(x => x.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(8);
        });
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.InvoiceId).IsRequired();
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.ProviderTransactionReference).HasMaxLength(100).IsRequired();
        // Idempotency: unique provider transaction reference.
        builder.HasIndex(p => p.ProviderTransactionReference).IsUnique();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.ReceivedAtUtc).IsRequired();

        builder.ComplexProperty(p => p.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.ComplexProperty(p => p.AmountPaid, m =>
        {
            m.Property(x => x.Amount).HasColumnName("AmountPaid").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(8);
        });

        builder.Property(p => p.RowVersion).IsConcurrencyToken();
    }
}

public sealed class ShaClaimConfiguration : IEntityTypeConfiguration<ShaClaim>
{
    public void Configure(EntityTypeBuilder<ShaClaim> builder)
    {
        builder.ToTable("sha_claims");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.InvoiceId).IsRequired();
        builder.Property(c => c.ShaClaimReference).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.ShaClaimReference).IsUnique();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.RejectionReason).HasMaxLength(500);
        builder.Property(c => c.SubmittedAtUtc).IsRequired();

        builder.ComplexProperty(c => c.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.Property(c => c.RowVersion).IsConcurrencyToken();
    }
}
