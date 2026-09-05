using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Jacana.HRMS.Api.Auth;

/// <summary>
/// Dual-scheme JWT: one validation pipeline, two delivery mechanisms.
///  • "Bearer" — reads the JWT from the Authorization header (mobile/server clients).
///  • "Cookie" — reads the same JWT from the HttpOnly "access_token" cookie (web client).
/// A policy scheme ("Smart") forwards to the right handler per request.
/// </summary>
public static class DualSchemeAuth
{
    public const string PolicyScheme = "Smart";
    public const string BearerScheme = "Bearer";
    public const string CookieScheme = "Cookie";
    public const string AccessTokenCookie = "access_token";
    public const string RefreshTokenCookie = "refresh_token";

    public static AuthenticationBuilder AddDualSchemeAuth(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        if (jwtKey.Length < 32)
            throw new InvalidOperationException($"Jwt:Key must be at least 32 characters (was {jwtKey.Length}).");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = "sub",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        return services.AddAuthentication(options =>
        {
            options.DefaultScheme = PolicyScheme;
            options.DefaultAuthenticateScheme = PolicyScheme;
            options.DefaultChallengeScheme = PolicyScheme;
        })
        .AddPolicyScheme(PolicyScheme, "JWT via Bearer header, query string (SignalR) or HttpOnly cookie", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                // SignalR WebSockets cannot set an Authorization header — the SPA
                // sends the JWT as ?access_token= on the hub path instead.
                if (context.Request.Path.StartsWithSegments("/hubs")
                    && !string.IsNullOrEmpty(context.Request.Query["access_token"]))
                    return BearerScheme;

                return context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? BearerScheme
                    : CookieScheme;
            };
        })
        .AddJwtBearer(BearerScheme, options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // SignalR (WebSockets/SSE) transports send the token in the
                    // query string because the browser cannot set headers there.
                    if (context.Request.Path.StartsWithSegments("/hubs"))
                    {
                        var queryToken = context.Request.Query["access_token"].ToString();
                        if (!string.IsNullOrEmpty(queryToken))
                            context.Token = queryToken;
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddJwtBearer(CookieScheme, options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var cookie = context.Request.Cookies[AccessTokenCookie];
                    if (!string.IsNullOrEmpty(cookie))
                        context.Token = cookie;
                    return Task.CompletedTask;
                }
            };
        });
    }
}
