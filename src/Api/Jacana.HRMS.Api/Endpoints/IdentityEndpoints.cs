using System.Security.Claims;
using Jacana.HRMS.Api.Auth;
using Jacana.HRMS.Api.Middleware;
using Jacana.Identity.Application;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Application.Features.Auth;
using Jacana.SharedKernel.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Minimal API endpoints: bind → dispatch to MediatR → map result → return.
/// No business logic lives here.
/// </summary>
public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapPost("/register", RegisterAsync)
            .RequireAuthorization(Permissions.Users.Register);

        group.MapGet("/csrf", GetCsrfTokenAsync);

        return app;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequestDto request, ISender sender, HttpContext httpContext, CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password, request.TotpCode), ct);
        if (result.IsFailure) return MapError(result.Error);
        if (result.Value.RequiresTwoFactor) return Results.Ok(result.Value);

        // Web client: tokens in HttpOnly cookies; bearer clients read the JSON body.
        var value = result.Value;
        if (isWebClient(httpContext) && value.AccessToken is not null)
        {
            SetAuthCookies(httpContext, value.AccessToken, value.RefreshToken!, true);
        }
        return Results.Ok(value);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequestDto request, ISender sender, HttpContext httpContext, CancellationToken ct)
    {
        var refreshToken = request.RefreshToken ?? httpContext.Request.Cookies[DualSchemeAuth.RefreshTokenCookie];
        var result = await sender.Send(new RefreshCommand(refreshToken), ct);
        if (result.IsFailure) return MapError(result.Error);

        if (isWebClient(httpContext) && result.Value.AccessToken is not null)
            SetAuthCookies(httpContext, result.Value.AccessToken, result.Value.RefreshToken!, true);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterUserCommand(
            request.FullName, request.Email, request.Phone, request.Password, request.RoleNames), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/users/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static IResult GetCsrfTokenAsync(HttpContext httpContext)
    {
        // The anti-forgery token equals the JWT id (jti) claim; the web client echoes it
        // back on state-changing requests. Simple synchronizer pattern.
        var csrf = httpContext.User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)
                   ?? Guid.NewGuid().ToString();
        httpContext.Response.Headers.Append(CsrfProtectionMiddleware.CsrfHeader, csrf);
        return Results.Ok(new { csrfToken = csrf });
    }

    private static void SetAuthCookies(HttpContext httpContext, string access, string refresh, bool httpOnly)
    {
        var options = new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };
        httpContext.Response.Cookies.Append(DualSchemeAuth.AccessTokenCookie, access, options);
        options.Expires = DateTimeOffset.UtcNow.AddDays(7);
        httpContext.Response.Cookies.Append(DualSchemeAuth.RefreshTokenCookie, refresh, options);
    }

    private static bool isWebClient(HttpContext context)
    {
        // The SPA is a bearer-token client and sends X-Auth-Mode: bearer on every
        // request (including login/refresh, where no Bearer header exists yet).
        // Never set HttpOnly cookies for it, or the cookie scheme will authenticate
        // the refresh request and the CSRF middleware will reject it.
        if (context.Request.Headers["X-Auth-Mode"].ToString().Equals("bearer", StringComparison.OrdinalIgnoreCase))
            return false;

        return !context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.Conflict => Results.Conflict(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
