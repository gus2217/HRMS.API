using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.SharedKernel.Infrastructure;

/// <summary>
/// Shared-kernel infrastructure registration. Registers the cross-cutting interceptors
/// (audit + outbox) that every module DbContext must attach, so soft-delete, RowVersion,
/// audit trail, and outbox capture are applied uniformly.
/// </summary>
public static class SharedKernelInfrastructureExtensions
{
    public static IServiceCollection AddSharedKernelInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<AuditingSaveChangesInterceptor>();
        services.AddSingleton<OutboxInterceptor>();
        return services;
    }

    /// <summary>Configures a module DbContext with Npgsql + the shared interceptors.</summary>
    public static DbContextOptionsBuilder UseJacanaPostgres(
        this DbContextOptionsBuilder options, string connectionString, IServiceProvider sp)
    {
        options.UseNpgsql(connectionString);
        options.AddInterceptors(
            sp.GetRequiredService<AuditingSaveChangesInterceptor>(),
            sp.GetRequiredService<OutboxInterceptor>());
        return options;
    }
}
