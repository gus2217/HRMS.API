using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Billing.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new BillingDbContext(options);
    }
}
