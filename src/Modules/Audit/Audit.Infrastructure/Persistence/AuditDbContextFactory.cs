using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Audit.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new AuditDbContext(options);
    }
}
