# Jacana HRMS

A production-grade Hospital Management System backend — a **.NET 8 modular monolith** built with Clean Architecture and CQRS. Eleven domain modules, one deployable API, one Postgres database (schema-per-module), a full audit trail, and fine-grained permission-based authorization.

> **Frontend:** the hospital UI is a separate SPA ([`jacana-ui`](https://github.com/gus2217/HRMS.UI)) that consumes this API under `/api/v1`. See [API surface](#api-surface) below.

---

## Table of Contents

- [Highlights](#highlights)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Modules](#modules)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Infrastructure (Docker)](#infrastructure-docker)
  - [Run the API](#run-the-api)
  - [Seed the database](#seed-the-database)
- [Authentication & Authorization](#authentication--authorization)
- [API Surface](#api-surface)
- [Configuration](#configuration)
- [Testing](#testing)
- [Known Issues & Notes](#known-issues--notes)

---

## Highlights

- **Modular monolith** — 11 modules that compile together, deploy as one process, but keep strict boundaries enforced by architecture tests.
- **Clean Architecture** — Domain / Application / Infrastructure / API layers; dependencies point inward; domain references nothing beyond `SharedKernel.Domain` + BCL.
- **CQRS with MediatR** — commands, queries, validators, and cross-cutting behaviors (validation, authorization, caching, logging, performance, transactions, outbox).
- **One database, many schemas** — each module owns its schema (`clinical.*`, `billing.*`, `inventory.*`, …). 11 EF Core `DbContext`s, migrated in dependency order.
- **Permission-based authorization** — **28 granular permissions** (`Patient.Register`, `Clinical.RecordDiagnosis`, `Queue.Accept`, `Appointment.Approve`, …) mapped to 11 roles. UI and API enforce permissions, never roles directly.
- **Security** — Argon2 password hashing, TOTP two-factor support, refresh-token rotation, dual-scheme auth (HttpOnly cookies for browser clients, bearer tokens for SPAs), AES-GCM field-level encryption for sensitive patient data.
- **Observability** — structured logging, OpenTelemetry tracing (OTLP), Hangfire background jobs with dashboard, `/health` endpoint.
- **Audit trail** — every create/update/delete across modules is captured with before/after values (interceptor-based, no code sprinkling).
- **Outbox pattern** — domain events dispatched reliably through an outbox table (auto-billing, notifications, and the live draft bill all flow through it).
- **81 automated tests** — 61 unit tests across all modules + 20 architecture tests enforcing layering and dependency rules.

## Tech Stack

| Area | Choice |
|---|---|
| Runtime | .NET 8 (LTS) |
| Language | C# 12 |
| Web | ASP.NET Core Minimal APIs |
| ORM | EF Core 8 (Npgsql 8.0.6 pinned) |
| Database | PostgreSQL 16 (schema-per-module) |
| Cache | Redis 7 |
| CQRS / MediatR | MediatR 12 + custom pipeline behaviors |
| Auth | Custom JWT (dual-scheme) + Argon2 + TOTP |
| Background jobs | Hangfire (in-process, Postgres storage) |
| Observability | OpenTelemetry (OTLP) + Serilog structured logs |
| Testing | xUnit + NetArchTest |

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Jacana.HRMS.Api                       │
│  Minimal API endpoints · Swagger · JWT/Cookie dual scheme    │
│  Hangfire server · OTel · Global exception handling          │
└───────────────┬──────────────────────────────┬──────────────┘
                │                              │
┌───────────────▼──────────────┐ ┌─────────────▼─────────────┐
│        Application           │ │        Infrastructure     │
│  Commands · Queries · DTOs   │ │  EF Core DbContexts ·     │
│  Validators · Behaviors      │ │  Repositories · Hashers   │
│  Domain-event handlers       │ │  Outbox · Audit interceptor│
└───────────────┬──────────────┘ └─────────────┬─────────────┘
                │                              │
┌───────────────▼──────────────────────────────▼─────────────┐
│                          Domain                              │
│  Entities · Value Objects · Enums · Domain Events · Errors  │
│  Zero references outside SharedKernel.Domain + BCL           │
└──────────────────────────────────────────────────────────────┘
                │
┌───────────────▼─────────────────────────────────────────────┐
│                      SharedKernel                            │
│  Domain (Entity, AggregateRoot, ValueObject, Result, Error) │
│  Application (behaviors, abstractions)                      │
│  Infrastructure (outbox, audit, encryption, time, cache)    │
└──────────────────────────────────────────────────────────────┘
```

### Cross-cutting behaviors (pipeline)

Requests flow through MediatR behaviors in order:

1. **ValidationBehavior** — FluentValidation validators
2. **AuthorizationBehavior** — permission checks
3. **TransactionBehavior** — resolves all `IUnitOfWork`s, commits only contexts with tracked changes
4. **CachingBehavior** — query cache (Redis) with invalidation
5. **LoggingBehavior / PerformanceBehavior** — structured logs + slow-query warnings

### Key infrastructure decisions

- **TransactionBehavior commits per-DbContext** — a cross-module command (e.g. dispense → stock movement) touches several contexts; each context commits its own changes atomically within the request scope.
- **Handlers map aggregates in memory** — command handlers return DTOs mapped from the in-memory aggregate *after* mutation, never by re-querying the DB before the transaction commits (eliminates a whole class of phantom-404 bugs).
- **Repositories mark new children `Added`** — `MarkNewChildrenAdded` marks detached client-keyed children `Added` so EF emits INSERTs, not phantom UPDATEs (the source of `DbUpdateConcurrencyException` "0 rows affected" bugs).

## Project Structure

```
Jacana.HRMS.sln
├── src/
│   ├── Api/Jacana.HRMS.Api/              # Host: endpoints, auth, middleware
│   └── Modules/
│       ├── Identity/                     # users, roles, permissions, tokens
│       ├── PatientRegistration/          # patients, allergies, consents, NOK
│       ├── Clinical/                     # consultations, queue, appointments
│       ├── Inventory/                    # drug catalog, stock batches, movements
│       ├── Pharmacy/                     # prescriptions, dispense records
│       ├── Laboratory/                   # lab orders, test items, results
│       ├── Billing/                      # invoices, payments, SHA claims
│       ├── Inpatient/                    # admissions, ward notes, discharge
│       ├── Notifications/                # in-app notifications
│       ├── Audit/                        # audit log read model
│       └── Reporting/                    # dashboards & analytical queries
├── tests/
│   ├── UnitTests/                        # 61 tests across all modules
│   └── ArchitectureTests/                # 20 layer/dependency rules
├── tools/Jacana.HRMS.DbInitializer/      # migrate-all + seed console tool
├── docker-compose.yml                    # Redis 7 + OTel (Postgres is external)
└── otel-collector-config.yaml
```

Every module follows the same template:

```
Module/
├── Module.Domain/          # entities, value objects, enums, domain events
├── Module.Application/     # DTOs, commands, queries, handlers, validators
└── Module.Infrastructure/  # DbContext, configurations, repositories, DI
```

## Modules

| Module | Responsibility |
|---|---|
| **Identity** | Users, roles, permission grants, JWT issuance/refresh, TOTP, login audit |
| **PatientRegistration** | Patient records (phone/ID duplicate detection), allergies, consents, next-of-kin |
| **Clinical** | Consultations (guarded status workflow + direct start), triage vitals, clinical notes, ICD diagnoses, structured documentation, consultation queue (clinic allocation + tokens), appointments (recurring series + follow-up/check-up linkage) |
| **Inventory** | Drug catalog (with live available quantity), stock batches with expiry, receive/adjust/dispense movements, low-stock alerts |
| **Pharmacy** | Prescriptions with line items (stock-validated against inventory), dispense with FEFO stock deduction |
| **Laboratory** | Lab orders, test items, result recording with abnormal flags |
| **Billing** | Invoices with line items, payments, SHA (Social Health Authority) claims; auto-billing via domain events incl. a live draft bill opened at consultation start |
| **Inpatient** | Admissions, bed/ward occupancy, ward notes, discharge |
| **Notifications** | In-app notification messages |
| **Audit** | Read model over the shared audit log |
| **Reporting** | Facility dashboard summary + analytical reports |

## Getting Started

### Prerequisites

- .NET 8 SDK
- An external PostgreSQL server (host/port/creds via `DB_HOST`/`DB_PORT`/`DB_NAME`/`DB_USER`/`DB_PASS`)
- Redis 7 (or the Docker container)

### Infrastructure (Docker)

Postgres is **not** part of the Docker stack — it runs on an external server.
Redis + the OTel collector run in containers:

```bash
docker compose up -d          # redis:7 + otel-collector
```

Connection defaults (override in `.env` or via `DB_*` env vars):

| Setting | Default |
|---|---|
| `DB_HOST` | `localhost` |
| `DB_NAME` | `jacana_hrms` |
| `DB_USER` | `jacana` |
| `DB_PASS` | `jacana` |
| `DB_PORT` | `5432` |

### Run the API

```bash
cd src/Api/Jacana.HRMS.Api
dotnet run                    # http://localhost:5099 (see launchSettings)
```

- Swagger UI: `http://localhost:5099/swagger`
- Health check: `http://localhost:5099/health`

### Seed the database

The solution ships a console tool that **migrates all 11 contexts in dependency order** and seeds permissions, roles, and users:

```bash
cd tools/Jacana.HRMS.DbInitializer
dotnet run
```

Seeded roles (11) and users (password: `ChangeMe123!`):

| Email | Role |
|---|---|
| `admin@stfrancis.local` | System Administrator |
| `doctor@stfrancis.local` | Doctor |
| `nurse@stfrancis.local` | Nurse |
| `reception@stfrancis.local` | Receptionist |
| `lab@stfrancis.local` | Lab Technician |
| `pharmacist@stfrancis.local` | Pharmacist |
| `storekeeper@stfrancis.local` | Storekeeper |
| `accountant@stfrancis.local` | Accountant |
| `cashier@stfrancis.local` | Cashier |
| `records@stfrancis.local` | Records Officer |
| `itsupport@stfrancis.local` | IT Support |

> **⚠️ Change `ChangeMe123!` and the JWT/encryption keys before any non-local deployment.**

## Authentication & Authorization

### Dual-scheme auth

- **Cookie scheme** — browser-based clients get HttpOnly cookies (access + refresh). CSRF protection applies only here.
- **Bearer scheme** — SPAs send `Authorization: Bearer <token>` **and** `X-Auth-Mode: bearer` to opt out of cookie/CSRF handling.

### Flow

1. `POST /api/v1/auth/login` → `{ accessToken, refreshToken, requiresTwoFactor, roles, … }`
2. `POST /api/v1/auth/refresh` with the refresh token → rotated token pair
3. Access tokens expire after **15 minutes**; refresh tokens after **7 days** (configurable).

### Permissions

28 permissions across modules:

- **Patients** — `Patient.Register`, `Patient.View`, `Patient.Update`, `Patient.ConfidentialView`
- **Clinical** — `Clinical.Consult`, `Clinical.RecordDiagnosis`, `Clinical.View`
- **Queue** — `Queue.Create`, `Queue.View`, `Queue.Accept`
- **Appointment** — `Appointment.Create`, `Appointment.View`, `Appointment.Request`, `Appointment.Approve`
- **Inventory** — `Inventory.Receive`, `Inventory.Adjust`
- **Pharmacy** — `Pharmacy.Dispense`
- **Laboratory** — `Laboratory.Order`, `Laboratory.RecordResult`
- **Billing** — `Billing.IssueInvoice`, `Billing.RecordPayment`, `Billing.View`
- **Identity** — `Identity.User.View`, `Identity.User.Register`, `Identity.User.AssignRole`, `Identity.User.Suspend`, `Identity.Role.View`, `Identity.Role.Manage`

Roles are seeded as bundles of permissions. The Administrator role holds **all** permissions.

## API Surface

All endpoints are grouped under `/api/v1` (see `src/Api/Jacana.HRMS.Api/Endpoints/*`):

| Group | Base path | Key operations |
|---|---|---|
| Identity | `/api/v1/auth` | `login`, `refresh`, `register`, `csrf` |
| Patients | `/api/v1/patients` | register (duplicate detection), check (pre-check), search (paged, phone/name/ID), detail, update demographics, allergies, consents, clinical history, medical record |
| Clinical | `/api/v1/consultations` | start, detail, begin (start clinical work), triage, diagnosis, notes, documentation, referrals, complete |
| Queue | `/api/v1/queue` | create (clinic allocation + token), list, accept (→ registers consultation), cancel |
| Appointments | `/api/v1/appointments` | create (recurring), list, calendar, start/complete/cancel/no-show; `/api/v1/appointment-requests` create/list/approve/decline |
| Inventory | `/api/v1/inventory` | create drug, receive stock, adjust stock, stock levels, low stock |
| Pharmacy | `/api/v1/pharmacy` | create prescription, detail, dispense |
| Laboratory | `/api/v1/lab` | create order, detail, record result |
| Billing | `/api/v1/billing` | issue invoice, detail, record payment, submit SHA claim |
| Inpatient | `/api/v1/inpatient` | admit, detail, discharge, add ward note, ward occupancy |
| Audit | `/api/v1/audit` | paged audit log search (entity/user names resolved, not raw GUIDs) |
| Reporting | `/api/v1/reports` | dashboard summary, registrations, revenue-by-service, stock levels, SHA claims, clinician workload (clinician names resolved) |

Responses use RFC 7807 problem details for errors (`{ error, detail, duplicateCandidates? }`); enums bind from their **string names** (`"Female"`, `"Cash"`), not numeric values.

## Configuration

`appsettings.json` (override via env vars / user secrets in production):

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jacana_hrms;Username=jacana;Password=jacana"
  },
  "Redis":     { "ConnectionString": "localhost:6379", "Database": 0, "KeyPrefix": "jacana:" },
  "Jwt":       { "Issuer": "jacana-hrms", "Audience": "jacana-clients",
                 "Key": "change-me-32-characters-minimum!!",
                 "AccessTokenMinutes": 15, "RefreshTokenDays": 7 },
  "OpenTelemetry": { "Endpoint": "http://localhost:4317" },
  "Security":  { "EncryptionKey": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=" }
}
```

**Production must override:** `Jwt:Key`, `Security:EncryptionKey`, DB credentials.

## Testing

```bash
dotnet test
```

- **61 unit tests** — domain state machines (consultation transitions, invoice lifecycle, stock movements, prescriptions), value objects, permission logic.
- **20 architecture tests** — NetArchTest rules: domain references nothing outside `SharedKernel.Domain`; Application references no Infrastructure; banned symbols (`BannedSymbols.txt`); public-setter and `Result/Wait` bans.

## Known Issues & Notes

- Migrations were regenerated at some point with new timestamps (OwnsOne→ComplexProperty, RowVersion→concurrency token) — a DB reset (`dotnet ef database drop` / `down -v`) is required when switching between old and new migration snapshots.
- `dotnet-ef` is pinned to **8.0.11** via `.config/dotnet-tools.json` (`dotnet tool restore`); Npgsql is pinned to **8.0.6** — the EF Core 8 provider line (newer Npgsql 9.x breaks EF Core 8 with a `TypeLoadException`).
- The auto-billed draft invoice and outbox-driven notifications are delivered by the Hangfire dispatcher (default every 10s), so cross-module effects (bill lines, notifications) can lag a request by up to ~10s. Trade the transactional-outbox reliability for synchronous dispatch if sub-second latency is required.

---

© Jacana Health Systems. Internal use.
