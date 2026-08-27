using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace Jacana.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Change-tracking helpers shared by module repositories.
///
/// <para>
/// EF Core's DetectChanges classifies a new child with a client-generated
/// (non-default) key discovered through a tracked aggregate's collection as
/// <c>Modified</c> — it cannot distinguish "brand-new child" from "existing row
/// that changed". The result is an UPDATE ... WHERE Id = &lt;new-guid&gt; that
/// affects 0 rows, surfacing as a <c>DbUpdateConcurrencyException</c>.
/// </para>
///
/// <para>
/// The fix is to explicitly mark new children <c>Added</c> while they are still
/// <c>Detached</c> — i.e. BEFORE anything triggers DetectChanges. Repositories
/// call <see cref="MarkNewChildrenAdded"/> inside <c>UpdateAsync</c>, which runs
/// before the unit-of-work's <c>HasChanges</c>/<c>SaveChanges</c>.
/// </para>
/// </summary>
public static class ChangeTrackingExtensions
{
    /// <summary>
    /// Marks every <c>Detached</c> entity reachable through the aggregate's
    /// enumerable properties as <c>Added</c>, so EF emits INSERTs instead of
    /// phantom UPDATEs. Only mapped entity types are touched (unmapped
    /// collections such as domain events are skipped).
    /// </summary>
    public static void MarkNewChildrenAdded(this DbContext db, object aggregate)
    {
        // DetectChanges would classify a client-keyed new child as Modified the
        // moment any EF API touches the graph. Disable it for the walk so the
        // children are still Detached when we check, then mark them Added.
        var autoDetect = db.ChangeTracker.AutoDetectChangesEnabled;
        db.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var prop in aggregate.GetType().GetProperties())
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                if (prop.PropertyType == typeof(string)) continue;
                if (!typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)) continue;
                if (prop.GetValue(aggregate) is not IEnumerable collection) continue;

                foreach (var child in collection)
                {
                    if (child is null) continue;
                    // Only entities mapped to a table can be tracked.
                    if (db.Model.FindEntityType(child.GetType()) is null) continue;

                    var entry = db.Entry(child);
                    if (entry.State == EntityState.Detached)
                        entry.State = EntityState.Added;
                }
            }
        }
        finally
        {
            db.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
        }
    }
}
