using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Infrastructure.Persistence;
using Jacana.Identity.Infrastructure.Repositories;
using Jacana.Identity.Infrastructure.Security;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jacana.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddSharedKernelInfrastructure();
        services.AddDbContext<IdentityDbContext>((sp, options) =>
            options.UseJacanaPostgres(connectionString, sp));

        services.AddScoped<IUnitOfWork, DbContextUnitOfWork<IdentityDbContext>>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
