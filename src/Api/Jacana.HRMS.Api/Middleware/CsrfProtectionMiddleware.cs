using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Jacana.HRMS.Api.Middleware;

/// <summary>
/// CSRF protection for the cookie-delivered scheme. All state-changing requests
/// (POST/PUT/PATCH/DELETE) authenticated via the HttpOnly cookie must present a
/// matching anti-forgery token (header "X-CSRF-TOKEN"). Bearer-authenticated
/// requests are exempt — no ambient cookie exists to forge.
/// </summary>
public sealed class CsrfProtectionMiddleware(RequestDelegate next)
{
    private static readonly string[] SafeMethods = ["GET", "HEAD", "OPTIONS", "TRACE"];
    public const string CsrfHeader = "X-CSRF-TOKEN";
    public const string CsrfClaim = "csrf";

    public async Task InvokeAsync(HttpContext context)
    {
        if (SafeMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Exempt bearer requests (no cookie).
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // SignalR negotiate/transports: the SPA authenticates the hub with the
        // JWT via ?access_token= (browsers cannot set headers on WebSockets), and
        // SignalR has its own CSRF protections (the token itself is the defence).
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var supplied = context.Request.Headers[CsrfHeader].ToString();
            var claim = context.User.FindFirstValue(CsrfClaim);

            if (string.IsNullOrEmpty(supplied) || string.IsNullOrEmpty(claim) || supplied != claim)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Anti-forgery token missing or invalid." });
                return;
            }
        }

        await next(context);
    }
}
