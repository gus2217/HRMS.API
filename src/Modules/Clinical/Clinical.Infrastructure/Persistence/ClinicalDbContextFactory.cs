using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Clinical.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class ClinicalDbContextFactory : IDesignTimeDbContextFactory<ClinicalDbContext>
{
    public ClinicalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClinicalDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new ClinicalDbContext(options);
    }
}
