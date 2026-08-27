using Jacana.Pharmacy.Application.Abstractions;
using Jacana.Pharmacy.Infrastructure.Persistence;
using Jacana.Pharmacy.Infrastructure.Repositories;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Pharmacy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPharmacyInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<PharmacyDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<PharmacyDbContext>>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IDispenseRecordRepository, DispenseRecordRepository>();

        return services;
    }
}
