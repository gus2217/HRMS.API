using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Infrastructure.Persistence;
using Jacana.Notifications.Infrastructure.Repositories;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<NotificationsDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<NotificationsDbContext>>();
        services.AddScoped<INotificationMessageRepository, NotificationMessageRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();

        return services;
    }
}
