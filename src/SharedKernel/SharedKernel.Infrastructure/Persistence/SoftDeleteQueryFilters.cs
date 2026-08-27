using System.Reflection;
using Jacana.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jacana.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Applies the soft-delete global query filter to every entity implementing
/// <see cref="ISoftDelete"/> — structural, not per-entity opt-in.
/// </summary>
public static class SoftDeleteQueryFilters
{
    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        var configure = typeof(SoftDeleteQueryFilters)
            .GetMethod(nameof(ConfigureFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)) continue;
            configure.MakeGenericMethod(entityType.ClrType).Invoke(null, [modelBuilder]);
        }
    }

    private static void ConfigureFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
