using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Infrastructure.Persistence;
using Jacana.Clinical.Infrastructure.Repositories;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Clinical.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddClinicalInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<ClinicalDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<ClinicalDbContext>>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IQueueEntryRepository, QueueEntryRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentRequestRepository, AppointmentRequestRepository>();
        services.AddScoped<IPatientClinicalRepository, PatientClinicalRepository>();

        return services;
    }
}
