using Jacana.Laboratory.Application.Abstractions;
using Jacana.Laboratory.Infrastructure.Persistence;
using Jacana.Laboratory.Infrastructure.Repositories;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Laboratory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLaboratoryInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<LaboratoryDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<LaboratoryDbContext>>();
        services.AddScoped<ILabOrderRepository, LabOrderRepository>();

        return services;
    }
}
