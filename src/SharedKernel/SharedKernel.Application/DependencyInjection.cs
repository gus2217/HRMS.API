using System.Reflection;
using FluentValidation;
using Jacana.SharedKernel.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.SharedKernel.Application;

public static class DependencyInjection
{
    /// <summary>Registers MediatR + the standard pipeline behaviors for a module's Application assembly.</summary>
    public static IServiceCollection AddApplicationPipeline(this IServiceCollection services, Assembly applicationAssembly)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
