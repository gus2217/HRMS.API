using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Inventory.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new InventoryDbContext(options);
    }
}
