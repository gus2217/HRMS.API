using Microsoft.AspNetCore.Authorization;
using Jacana.Identity.Application;

namespace Jacana.HRMS.Api.Auth;

/// <summary>
/// Permission policy provider. Maps a permission code (e.g. "Patient.Register") to a
/// named authorization policy that resolves against the user's permission claims.
/// No role-name string checks anywhere.
/// </summary>
public static class PermissionPolicies
{
    public const string PermissionClaim = "permission";

    public static AuthorizationOptions AddPermissionPolicies(this AuthorizationOptions options)
    {
        // Register one policy per known permission code.
        foreach (var code in AllPermissionCodes)
        {
            options.AddPolicy(code, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(PermissionClaim, code));
        }
        return options;
    }

    private static readonly string[] AllPermissionCodes =
    [
        Permissions.Users.View, Permissions.Users.Register, Permissions.Users.AssignRole, Permissions.Users.Suspend,
        Permissions.Roles.View, Permissions.Roles.Manage,
        Permissions.Patients.Register, Permissions.Patients.View, Permissions.Patients.Update, Permissions.Patients.ConfidentialView,
        Permissions.Billing.IssueInvoice, Permissions.Billing.RecordPayment, Permissions.Billing.View,
        Permissions.Clinical.Consult, Permissions.Clinical.RecordDiagnosis, Permissions.Clinical.View,
        Permissions.Lab.Order, Permissions.Lab.RecordResult,
        Permissions.Pharmacy.Dispense,
        Permissions.Inventory.Receive, Permissions.Inventory.Adjust
    ];
}
