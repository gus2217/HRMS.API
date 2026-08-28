using Jacana.Audit.Infrastructure.Persistence;
using Jacana.Billing.Infrastructure.Persistence;
using Jacana.Clinical.Infrastructure.Persistence;
using Jacana.Identity.Domain;
using Jacana.Identity.Infrastructure.Persistence;
using Jacana.Identity.Infrastructure.Security;
using Jacana.Inpatient.Infrastructure.Persistence;
using Jacana.Inventory.Infrastructure.Persistence;
using Jacana.Laboratory.Infrastructure.Persistence;
using Jacana.Notifications.Infrastructure.Persistence;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.Pharmacy.Infrastructure.Persistence;
using Jacana.SharedKernel.Domain;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Outbox;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Jacana.SharedKernel.Infrastructure.Security;
using Jacana.SharedKernel.Infrastructure.Time;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

// ─────────────────────────────────────────────────────────────────────────────
// Jacana HRMS — database initializer.
//
// Applies all 11 module migrations in dependency order (shared outbox + audit
// schemas first), then seeds the Identity module: permissions, roles, and one
// login per role. Idempotent — safe to re-run.
//
// Usage:  dotnet run --project tools/Jacana.HRMS.DbInitializer [connectionString]
//         (defaults to localhost jacana/jacana, matching docker-compose.yml)
// ─────────────────────────────────────────────────────────────────────────────

var connectionString = args.FirstOrDefault()
    ?? "Host=localhost;Port=5432;Database=jacana_hrms;Username=jacana;Password=jacana";

const string seedPassword = "ChangeMe123!";

Console.WriteLine("Jacana HRMS — DB initializer");
Console.WriteLine("─────────────────────────────");

// ── 1. Migrations (dependency order: shared schemas first) ───────────────────
Console.WriteLine("Applying migrations…");

Migrate<OutboxDbContext>(connectionString);     // outbox schema (shared)
Migrate<AuditDbContext>(connectionString);      // audit schema (shared)
Migrate<IdentityDbContext>(connectionString);   // identity schema
MigratePatient(connectionString);               // patient_registration (needs encryptor)
Migrate<ClinicalDbContext>(connectionString);   // clinical
Migrate<InventoryDbContext>(connectionString);  // inventory
Migrate<PharmacyDbContext>(connectionString);   // pharmacy
Migrate<LaboratoryDbContext>(connectionString); // laboratory
Migrate<BillingDbContext>(connectionString);    // billing
Migrate<InpatientDbContext>(connectionString);  // inpatient
Migrate<NotificationsDbContext>(connectionString); // notifications

Console.WriteLine("Migrations complete.");
Console.WriteLine();

// ── 2. Seed Identity (permissions, roles, users) ──────────────────────────────
Console.WriteLine("Seeding identity data…");
await SeedIdentityAsync(connectionString, seedPassword);

Console.WriteLine();
Console.WriteLine("Done. Seed credentials (password for all): " + seedPassword);
Console.WriteLine("  admin@stfrancis.local          — Administrator");
Console.WriteLine("  doctor@stfrancis.local         — Doctor");
Console.WriteLine("  nurse@stfrancis.local          — Nurse");
Console.WriteLine("  reception@stfrancis.local      — Receptionist");
Console.WriteLine("  lab@stfrancis.local            — LabTechnician");
Console.WriteLine("  pharmacist@stfrancis.local     — Pharmacist");
Console.WriteLine("  storekeeper@stfrancis.local    — StoreKeeper");
Console.WriteLine("  accountant@stfrancis.local     — Accountant");
Console.WriteLine("  cashier@stfrancis.local        — Cashier");
Console.WriteLine("  records@stfrancis.local        — RecordsOfficer");
Console.WriteLine("  itsupport@stfrancis.local      — ITSupport");
Console.WriteLine();
Console.WriteLine("⚠ Change these passwords before any shared/hosted environment.");

// ─────────────────────────────────────────────────────────────────────────────
// Migrate a single DbContext using its DbContextOptions<T> constructor.
// ─────────────────────────────────────────────────────────────────────────────
static void Migrate<TContext>(string cs) where TContext : DbContext
{
    var options = new DbContextOptionsBuilder<TContext>()
        .UseNpgsql(cs)
        .Options;
    using var db = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
    db.Database.Migrate();
    Console.WriteLine($"  ✓ {typeof(TContext).Name}");
}

// PatientDbContext's constructor takes an IValueEncryptor (NationalId is encrypted
// at rest), so it can't use the single-arg Activator path above.
static void MigratePatient(string cs)
{
    var options = new DbContextOptionsBuilder<PatientDbContext>()
        .UseNpgsql(cs)
        .Options;
    using var db = new PatientDbContext(options, new AesGcmValueEncryptor(DesignTime.DevEncryptionKey));
    db.Database.Migrate();
    Console.WriteLine("  ✓ PatientDbContext");
}

// ─────────────────────────────────────────────────────────────────────────────
// Seed permissions → roles → users into the identity schema. Idempotent.
// ─────────────────────────────────────────────────────────────────────────────
static async Task SeedIdentityAsync(string cs, string password)
{
    // Attach the shared interceptors so audit fields + RowVersion are stamped
    // exactly as they are at runtime (a console host has no HTTP context → CreatedBy = Guid.Empty).
    IHttpContextAccessor httpAccessor = new NullHttpContextAccessor();
    var clock = new SystemClock();
    var auditInterceptor = new AuditingSaveChangesInterceptor(httpAccessor, clock);
    var outboxInterceptor = new OutboxInterceptor();

    var options = new DbContextOptionsBuilder<IdentityDbContext>()
        .UseNpgsql(cs)
        .AddInterceptors(auditInterceptor, outboxInterceptor)
        .Options;

    await using var db = new IdentityDbContext(options);

    // ── Permissions ───────────────────────────────────────────────────────────
    var permissionCodes = new (string Code, string Description)[]
    {
        ("Identity.User.View", "View users"),
        ("Identity.User.Register", "Register users"),
        ("Identity.User.AssignRole", "Assign roles to users"),
        ("Identity.User.Suspend", "Suspend/reactivate users"),
        ("Identity.Role.View", "View roles"),
        ("Identity.Role.Manage", "Create and manage roles"),
        ("Patient.Register", "Register patients"),
        ("Patient.View", "View patients"),
        ("Patient.Update", "Update patient records"),
        ("Patient.ConfidentialView", "View confidential patient data (phone, SHA, address, next of kin)"),
        ("Billing.IssueInvoice", "Issue invoices"),
        ("Billing.RecordPayment", "Record payments"),
        ("Billing.View", "View billing data"),
        ("Clinical.Consult", "Start and manage consultations"),
        ("Clinical.RecordDiagnosis", "Record diagnoses"),
        ("Clinical.View", "View clinical records"),
        ("Laboratory.Order", "Order lab tests"),
        ("Laboratory.RecordResult", "Record lab results"),
        ("Pharmacy.Dispense", "Dispense prescriptions"),
        ("Inventory.Receive", "Receive stock"),
        ("Inventory.Adjust", "Adjust stock levels"),
    };

    var permissions = new Dictionary<string, Permission>(StringComparer.Ordinal);
    foreach (var (code, description) in permissionCodes)
    {
        var existing = await db.Permissions
            .FirstOrDefaultAsync(p => p.Code == code);
        if (existing is not null)
        {
            permissions[code] = existing;
            continue;
        }
        var created = Permission.Create(Guid.NewGuid(), code, description).Value;
        db.Permissions.Add(created);
        permissions[code] = created;
    }
    await db.SaveChangesAsync();
    Console.WriteLine($"  ✓ {permissionCodes.Length} permissions");

    // ── Roles + permission grants ─────────────────────────────────────────────
    var roleGrants = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        [nameof(SystemRole.Administrator)] = permissionCodes.Select(p => p.Code).ToArray(),
        [nameof(SystemRole.Doctor)] = new[]
        {
            "Clinical.Consult", "Clinical.RecordDiagnosis", "Clinical.View",
            "Patient.View", "Patient.Register", "Patient.ConfidentialView",
            "Laboratory.Order", "Pharmacy.Dispense",
        },
        [nameof(SystemRole.Nurse)] = new[]
        {
            "Patient.View", "Patient.Register", "Clinical.View", "Laboratory.Order",
            "Patient.ConfidentialView", // triage/notes need contact + NOK context
            "Clinical.Consult", // triage, begin phase, clinical notes
        },
        [nameof(SystemRole.Receptionist)] = new[]
        {
            "Patient.Register", "Patient.View", "Patient.Update", "Patient.ConfidentialView",
            "Billing.IssueInvoice", "Billing.View",
        },
        [nameof(SystemRole.LabTechnician)] = new[]
        {
            "Laboratory.Order", "Laboratory.RecordResult", "Patient.View",
        },
        [nameof(SystemRole.Pharmacist)] = new[] { "Pharmacy.Dispense", "Patient.View" },
        [nameof(SystemRole.StoreKeeper)] = new[] { "Inventory.Receive", "Inventory.Adjust" },
        [nameof(SystemRole.Accountant)] = new[]
        {
            "Billing.IssueInvoice", "Billing.RecordPayment", "Billing.View",
        },
        [nameof(SystemRole.Cashier)] = new[] { "Billing.RecordPayment", "Billing.View" },
        [nameof(SystemRole.RecordsOfficer)] = new[] { "Patient.View", "Patient.Update", "Patient.ConfidentialView" },
        [nameof(SystemRole.ITSupport)] = new[]
        {
            "Identity.User.View", "Identity.User.Register",
            "Identity.User.AssignRole", "Identity.User.Suspend",
            "Identity.Role.View", "Identity.Role.Manage",
        },
    };

    var roles = new Dictionary<string, Role>(StringComparer.Ordinal);
    foreach (var (roleName, codes) in roleGrants)
    {
        var existing = await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == roleName);
        var role = existing ?? Role.Create(Guid.NewGuid(), roleName).Value;

        if (existing is null)
        {
            db.Roles.Add(role);
            foreach (var code in codes)
                role.Grant(permissions[code]);
        }
        else
        {
            // Idempotent re-seed: sync the role to the canonical grant set so
            // re-running the initializer applies seed changes to existing roles.
            var currentCodes = role.Permissions.Select(p => p.Permission.Code).ToHashSet();
            foreach (var code in codes)
            {
                if (!currentCodes.Contains(code))
                    role.Grant(permissions[code]);
            }
        }
        roles[roleName] = role;
    }
    await db.SaveChangesAsync();
    Console.WriteLine($"  ✓ {roles.Count} roles");

    // ── Users (one per role, fixed facility) ──────────────────────────────────
    var facilityId = FacilityId.From(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    var hasher = new Argon2PasswordHasher();

    var users = new (string Email, string FullName, string Role, string Phone)[]
    {
        ("admin@stfrancis.local",       "System Administrator", nameof(SystemRole.Administrator), "+254700000001"),
        ("doctor@stfrancis.local",      "Dr. Jane Mwangi",      nameof(SystemRole.Doctor),        "+254700000002"),
        ("nurse@stfrancis.local",       "Nurse Grace Wanjiku",  nameof(SystemRole.Nurse),         "+254700000003"),
        ("reception@stfrancis.local",   "Receptionist Faith A.", nameof(SystemRole.Receptionist),  "+254700000004"),
        ("lab@stfrancis.local",         "Lab Tech Brian Otieno", nameof(SystemRole.LabTechnician), "+254700000005"),
        ("pharmacist@stfrancis.local",  "Pharmacist Sarah C.",  nameof(SystemRole.Pharmacist),     "+254700000006"),
        ("storekeeper@stfrancis.local", "Storekeeper James K.", nameof(SystemRole.StoreKeeper),    "+254700000007"),
        ("accountant@stfrancis.local",  "Accountant Kevin N.",  nameof(SystemRole.Accountant),     "+254700000008"),
        ("cashier@stfrancis.local",     "Cashier Mercy A.",     nameof(SystemRole.Cashier),        "+254700000009"),
        ("records@stfrancis.local",     "Records Officer Peter", nameof(SystemRole.RecordsOfficer), "+254700000010"),
        ("itsupport@stfrancis.local",   "IT Support Dennis B.", nameof(SystemRole.ITSupport),      "+254700000011"),
    };

    var createdCount = 0;
    foreach (var (email, fullName, roleName, phone) in users)
    {
        if (await db.Users.AnyAsync(u => u.Email == email))
            continue;

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
            throw new InvalidOperationException($"Invalid seed phone {phone}: {phoneResult.Error.Message}");

        var userResult = User.Register(
            Guid.NewGuid(), facilityId, fullName, email, phoneResult.Value, hasher.Hash(password));
        if (userResult.IsFailure)
            throw new InvalidOperationException($"Failed to seed {email}: {userResult.Error.Message}");

        var user = userResult.Value;
        user.AssignRole(roles[roleName]);
        db.Users.Add(user);
        createdCount++;
    }
    await db.SaveChangesAsync();
    Console.WriteLine(createdCount == 0
        ? "  ✓ users already seeded (skipped)"
        : $"  ✓ {createdCount} users");
}

// Minimal IHttpContextAccessor for a console host — there is no ambient HTTP
// request during seeding, so CurrentUser resolves to Guid.Empty (system seed).
internal sealed class NullHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}
