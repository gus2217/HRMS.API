using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Domain;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.PatientRegistration.Infrastructure.Repositories;
using Jacana.PatientRegistration.Infrastructure.Services;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.PatientRegistration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPatientRegistrationInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<PatientDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<PatientDbContext>>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDuplicatePatientDetectionService, DuplicatePatientDetectionService>();
        services.AddScoped<IPatientNumberGenerator, PatientNumberGenerator>();

        return services;
    }
}
