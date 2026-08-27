using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Laboratory.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class LaboratoryDbContextFactory : IDesignTimeDbContextFactory<LaboratoryDbContext>
{
    public LaboratoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LaboratoryDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new LaboratoryDbContext(options);
    }
}
