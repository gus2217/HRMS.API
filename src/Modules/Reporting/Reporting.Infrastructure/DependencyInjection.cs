using Jacana.Reporting.Application.Abstractions;
using Jacana.Reporting.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IReportingReadRepository>(_ =>
            new ReportingReadRepository(connectionString));

        return services;
    }
}
