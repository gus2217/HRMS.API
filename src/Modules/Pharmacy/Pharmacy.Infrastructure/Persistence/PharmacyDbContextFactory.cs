using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Pharmacy.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class PharmacyDbContextFactory : IDesignTimeDbContextFactory<PharmacyDbContext>
{
    public PharmacyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PharmacyDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new PharmacyDbContext(options);
    }
}
