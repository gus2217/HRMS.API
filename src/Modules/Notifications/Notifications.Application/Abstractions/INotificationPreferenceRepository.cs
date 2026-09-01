using Jacana.Notifications.Domain;

namespace Jacana.Notifications.Application.Abstractions;

/// <summary>Per-user, per-category delivery preferences (defaults-on when absent).</summary>
public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetAsync(Guid recipientUserId, NotificationCategory category, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationPreference>> GetByUserAsync(Guid recipientUserId, CancellationToken ct = default);
    Task AddAsync(NotificationPreference preference, CancellationToken ct = default);
    Task UpdateAsync(NotificationPreference preference, CancellationToken ct = default);

    /// <summary>
    /// Filters a candidate recipient list down to those who have in-app
    /// notifications enabled for the category (absent preference = enabled).
    /// </summary>
    Task<IReadOnlyList<Guid>> FilterInAppEnabledAsync(
        IReadOnlyCollection<Guid> recipientUserIds, NotificationCategory category, CancellationToken ct = default);
}
