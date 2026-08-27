using Jacana.SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.Notifications.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class NotificationsDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        return new NotificationsDbContext(options);
    }
}
