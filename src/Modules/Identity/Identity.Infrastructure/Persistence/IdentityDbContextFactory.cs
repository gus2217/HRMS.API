using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Identity.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new IdentityDbContext(options);
    }
}
