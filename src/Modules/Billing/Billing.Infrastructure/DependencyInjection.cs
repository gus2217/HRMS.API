using Jacana.Billing.Application.Abstractions;
using Jacana.Billing.Infrastructure.Persistence;
using Jacana.Billing.Infrastructure.Repositories;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBillingInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<BillingDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<BillingDbContext>>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IShaClaimRepository, ShaClaimRepository>();

        return services;
    }
}
