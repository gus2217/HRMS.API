using Jacana.Inventory.Application.Abstractions;
using Jacana.Inventory.Domain;
using Jacana.Inventory.Infrastructure.Persistence;
using Jacana.Inventory.Infrastructure.Repositories;
using Jacana.Inventory.Infrastructure.Services;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<InventoryDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<InventoryDbContext>>();
        services.AddScoped<IDrugRepository, DrugRepository>();
        services.AddScoped<IStockBatchRepository, StockBatchRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IInventoryStockQuery, InventoryStockQuery>();

        return services;
    }
}
