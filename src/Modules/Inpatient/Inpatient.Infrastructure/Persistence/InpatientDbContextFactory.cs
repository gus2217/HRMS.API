using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Inpatient.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class InpatientDbContextFactory : IDesignTimeDbContextFactory<InpatientDbContext>
{
    public InpatientDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InpatientDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new InpatientDbContext(options);
    }
}
