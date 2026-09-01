namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Resolves user IDs by role name (e.g. all Doctors, all Pharmacists) so a
/// notification handler can fan out an event to every user in a role without
/// referencing the Identity module's entities directly.
/// </summary>
public interface IUserRoleLookup
{
    Task<IReadOnlyCollection<Guid>> GetUserIdsByRolesAsync(
        IReadOnlyCollection<string> roleNames, CancellationToken ct = default);
}
