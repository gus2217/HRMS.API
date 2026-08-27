using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Infrastructure.Persistence;
using Jacana.Inpatient.Infrastructure.Repositories;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Inpatient.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInpatientInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<InpatientDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<InpatientDbContext>>();
        services.AddScoped<IAdmissionRepository, AdmissionRepository>();

        return services;
    }
}
