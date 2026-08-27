using System.Security.Claims;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;

namespace Jacana.SharedKernel.Infrastructure.Identity;

/// <summary>Claim types used across the system. Centralized to avoid magic strings.</summary>
public static class JwtClaimTypes
{
    public const string UserId = "sub";
    public const string FacilityId = "facility_id";
    public const string Role = "role";
    public const string Permission = "permission";
}

/// <summary>
/// Resolves the current user from the ambient HTTP context. Requires
/// <see cref="IHttpContextAccessor"/> registered at composition root.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal();

    public Guid UserId
    {
        get
        {
            var value = FirstValue(JwtClaimTypes.UserId) ?? FirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public FacilityId FacilityId
    {
        get
        {
            var value = FirstValue(JwtClaimTypes.FacilityId);
            return Guid.TryParse(value, out var id) ? FacilityId.From(id) : FacilityId.New();
        }
    }

    public IReadOnlyCollection<string> Roles =>
        Principal.FindAll(JwtClaimTypes.Role).Select(c => c.Value).ToArray();

    public IReadOnlyCollection<string> Permissions =>
        Principal.FindAll(JwtClaimTypes.Permission).Select(c => c.Value).ToArray();

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated ?? false;

    private string? FirstValue(string claimType) =>
        Principal.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
}
