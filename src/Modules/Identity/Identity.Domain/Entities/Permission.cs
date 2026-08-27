using Jacana.SharedKernel.Domain;

namespace Jacana.Identity.Domain;

/// <summary>
/// A named permission, e.g. "Patient.Register", "Billing.IssueInvoice".
/// Permissions are granted to roles, roles to users; handlers check the permission
/// code via named policy — never a hardcoded role-name string.
/// </summary>
public sealed class Permission : AggregateRoot<Guid>
{
    private Permission() { } // EF

    private Permission(Guid id, string code, string description) : base(id)
    {
        Code = code;
        Description = description;
    }

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public static Result<Permission> Create(Guid id, string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code)) return Error.Validation("Permission code is required.");
        return new Permission(id, code.Trim(), description ?? string.Empty);
    }
}
