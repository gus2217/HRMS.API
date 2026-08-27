using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.SharedKernel.Infrastructure.Outbox;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class OutboxDbContextFactory : IDesignTimeDbContextFactory<OutboxDbContext>
{
    public OutboxDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new OutboxDbContext(options);
    }
}
