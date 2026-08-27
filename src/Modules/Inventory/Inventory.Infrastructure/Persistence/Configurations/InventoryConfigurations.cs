using Jacana.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jacana.Inventory.Infrastructure.Persistence.Configurations;

public sealed class DrugConfiguration : IEntityTypeConfiguration<Drug>
{
    public void Configure(EntityTypeBuilder<Drug> builder)
    {
        builder.ToTable("drugs");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Code).HasMaxLength(32).IsRequired();
        builder.HasIndex(d => d.Code).IsUnique();
        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Form).HasMaxLength(50).IsRequired();
        builder.Property(d => d.ReorderLevel).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(16);

        builder.OwnsOne(d => d.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.OwnsOne(d => d.UnitPrice, m =>
        {
            m.Property(x => x.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(8);
        });

        builder.Property(d => d.RowVersion).IsRowVersion();
    }
}

public sealed class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> builder)
    {
        builder.ToTable("stock_batches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.DrugId).IsRequired();
        builder.Property(b => b.BatchNumber).HasMaxLength(64).IsRequired();
        builder.Property(b => b.QuantityOnHand).IsRequired();
        builder.Property(b => b.ExpiryDate).IsRequired();

        builder.OwnsOne(b => b.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.OwnsOne(b => b.UnitCost, m =>
        {
            m.Property(x => x.Amount).HasColumnName("UnitCost").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(8);
        });

        builder.Property(b => b.RowVersion).IsRowVersion();

        builder.HasMany(b => b.Movements).WithOne().HasForeignKey("StockBatchId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.Quantity).IsRequired();
        builder.Property(m => m.Reference).HasMaxLength(100);
        builder.Property(m => m.PerformedByUserId).IsRequired();
        builder.Property(m => m.MovementAtUtc).IsRequired();
    }
}

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.OwnsOne(s => s.FacilityId, f => f.Property(x => x.Value).HasColumnName("FacilityId").IsRequired());
        builder.OwnsOne(s => s.Phone, p => p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired());
        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}
