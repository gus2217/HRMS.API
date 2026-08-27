using Jacana.Audit.Application.Abstractions;
using Jacana.Audit.Infrastructure.Persistence;
using Jacana.Audit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAuditLogReadRepository, AuditLogReadRepository>();

        return services;
    }
}
