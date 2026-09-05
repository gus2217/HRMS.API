using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using Jacana.HRMS.Api;
using Jacana.HRMS.Api.Auth;
using Jacana.HRMS.Api.Endpoints;
using Jacana.HRMS.Api.Hubs;
using Jacana.HRMS.Api.Middleware;
using Jacana.Identity.Application;
using Jacana.Identity.Application.Features.Auth;
using Jacana.Identity.Infrastructure;
using Jacana.PatientRegistration.Application.Features.Patients;
using Jacana.PatientRegistration.Infrastructure;
using Jacana.Clinical.Application.Features.Consultations;
using Jacana.Clinical.Infrastructure;
using Jacana.Inventory.Application.Features.Inventory;
using Jacana.Inventory.Infrastructure;
using Jacana.Pharmacy.Application.Features.Pharmacy;
using Jacana.Pharmacy.Infrastructure;
using Jacana.Laboratory.Application.Features.Laboratory;
using Jacana.Laboratory.Infrastructure;
using Jacana.Billing.Application.Features.Billing;
using Jacana.Billing.Infrastructure;
using Jacana.Inpatient.Application.Features.Inpatient;
using Jacana.Inpatient.Infrastructure;
using Jacana.Notifications.Application;
using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Infrastructure;
using Jacana.Audit.Application.Features.Audit;
using Jacana.Audit.Infrastructure;
using Jacana.Reporting.Application.Features.Reporting;
using Jacana.Reporting.Infrastructure;
using Jacana.Identity.Infrastructure.Security;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Behaviors;
using Jacana.SharedKernel.Infrastructure.Caching;
using Jacana.SharedKernel.Infrastructure.Identity;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Services;
using Jacana.SharedKernel.Infrastructure.Time;
using Jacana.HRMS.Api.Storage;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Minimal APIs must bind enum values from their JSON string names (e.g. "Female",
// "Single") — the frontend sends strings, not integers.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
});

// ── Serilog structured logging ────────────────────────────────────────────────
builder.Host.UseSerilog((context, services, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

// ── OpenTelemetry tracing ─────────────────────────────────────────────────────
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("jacana-hrms"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]
            ?? "http://localhost:4317")));

// ── Shared kernel contracts ───────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddSingleton<IFileStorage>(_ => CreateFileStorage(builder.Configuration, builder.Environment));
builder.Services.AddSingleton<IPatientIdentityLookup>(_ =>
    new PatientIdentityLookup(connectionString));
builder.Services.AddSingleton<IUserIdentityLookup>(_ =>
    new UserIdentityLookup(connectionString));
builder.Services.AddSingleton<IUserRoleLookup>(_ =>
    new UserRoleLookup(connectionString));
builder.Services.AddSingleton<IBillingStatusLookup>(_ =>
    new BillingStatusLookup(connectionString));
builder.Services.AddSingleton<IValueEncryptor>(_ =>
    new Jacana.SharedKernel.Infrastructure.Security.AesGcmValueEncryptor(
        builder.Configuration["Security:EncryptionKey"]
        ?? throw new InvalidOperationException("Security:EncryptionKey is not configured.")));
builder.Services.AddSingleton<ICacheService>(_ =>
    new MemoryCacheService(new Microsoft.Extensions.Caching.Memory.MemoryCache(
        new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions())));
builder.Services.AddSingleton(PerformanceBehaviorOptions.Default);

// ── SignalR real-time notifications ───────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationPusher, SignalRNotificationPusher>();

// ── MediatR + pipeline behaviors (order: logging → validation → auth → tx → perf → cache) ──
builder.Services.AddApplicationPipeline(typeof(LoginCommand).Assembly);

// ── Identity module ───────────────────────────────────────────────────────────
builder.Services.AddIdentityInfrastructure(connectionString);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// ── Patient Registration module ───────────────────────────────────────────────
builder.Services.AddPatientRegistrationInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(RegisterPatientCommand).Assembly);

// ── Clinical module ───────────────────────────────────────────────────────────
builder.Services.AddClinicalInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(StartConsultationCommand).Assembly);

// ── Inventory + Pharmacy + Laboratory modules ─────────────────────────────────
builder.Services.AddInventoryInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(CreateDrugCommand).Assembly);
builder.Services.AddPharmacyInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(CreatePrescriptionCommand).Assembly);
builder.Services.AddLaboratoryInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(CreateLabOrderCommand).Assembly);

// ── Billing module ────────────────────────────────────────────────────────────
builder.Services.AddBillingInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(IssueInvoiceCommand).Assembly);

// ── Inpatient + Notifications + Audit modules ─────────────────────────────────
builder.Services.AddInpatientInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(AdmitPatientCommand).Assembly);
builder.Services.AddNotificationsInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(Jacana.Notifications.Application.DomainEventHandlers.ConsultationRequestedHandler).Assembly);
builder.Services.AddAuditInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(GetAuditLogQuery).Assembly);

// ── Reporting module (read-only) ─────────────────────────────────────────────
builder.Services.AddReportingInfrastructure(connectionString);
builder.Services.AddApplicationPipeline(typeof(DailyRegistrationsReportQuery).Assembly);

// ── Auth: dual-scheme JWT + permission policies ────────────────────────────────
builder.Services.AddDualSchemeAuth(builder.Configuration);
builder.Services.AddAuthorization(options => options.AddPermissionPolicies());

// ── Rate limiting on auth + public endpoints ──────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
    });
});

// ── Hangfire (background jobs + outbox dispatcher) ─────────────────────────────
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(o => o.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();
// OutboxDispatcher reads the shared outbox table via its own minimal DbContext.
builder.Services.AddDbContext<OutboxDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddScoped<OutboxDispatcher>();

// Auto-billing fees for consultations (config section "Billing").
builder.Services.Configure<Jacana.Billing.Application.Features.Billing.DomainEventHandlers.BillingFeeOptions>(
    builder.Configuration.GetSection("Billing"));

// ── Minimal API ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jacana HRMS", Version = "v1" });
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CsrfProtectionMiddleware>();

app.UseHangfireDashboard();

// Dispatch outbox messages (domain events → MediatR handlers) every 10 seconds.
RecurringJob.AddOrUpdate<OutboxDispatcher>(
    "outbox-dispatcher",
    d => d.DispatchAsync(20, CancellationToken.None),
    "*/10 * * * * *");

// ── Endpoints ──────────────────────────────────────────────────────────────────
app.MapIdentityEndpoints();
app.MapPatientEndpoints();
app.MapClinicalEndpoints();
app.MapPatientClinicalEndpoints();
app.MapFlagsAttachmentsOrdersEndpoints();
app.MapQueueEndpoints();
app.MapAppointmentEndpoints();
app.MapInventoryEndpoints();
app.MapPharmacyEndpoints();
app.MapLaboratoryEndpoints();
app.MapBillingEndpoints();
app.MapInpatientEndpoints();
app.MapAuditEndpoints();
app.MapNotificationEndpoints();
app.MapReportingEndpoints();

// ── SignalR real-time notifications ───────────────────────────────────────────
app.MapHub<NotificationsHub>("/hubs/notifications");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.Run();

/// <summary>
/// Storage backend selection — MinIO object storage when configured
/// (Storage:Provider=Minio), otherwise the local-disk fallback.
/// </summary>
static IFileStorage CreateFileStorage(IConfiguration configuration, IWebHostEnvironment environment)
{
    var provider = configuration["Storage:Provider"] ?? "Local";
    if (string.Equals(provider, "Minio", StringComparison.OrdinalIgnoreCase))
    {
        var endpoint = configuration["Storage:Minio:Endpoint"]
            ?? throw new InvalidOperationException("Storage:Minio:Endpoint is required when Storage:Provider=Minio.");
        return new MinioFileStorage(
            endpoint,
            configuration["Storage:Minio:AccessKey"] ?? string.Empty,
            configuration["Storage:Minio:SecretKey"] ?? string.Empty,
            configuration["Storage:Minio:Bucket"] ?? "jacana-media",
            bool.TryParse(configuration["Storage:Minio:UseSsl"], out var useSsl) && useSsl);
    }

    return new LocalFileStorage(configuration["Storage:AttachmentsPath"]
        ?? Path.Combine(environment.ContentRootPath, "attachments"));
}
