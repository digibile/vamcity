# VamCity Linux migration execution gate

Updated: 2026-10-15

## Non-negotiable data and deployment rules

- Keep the active Roofixo application, SQL Server database, existing public files, and real customer/loan records untouched while the Linux port is being validated.
- Never commit the SQL Server backup, .env, connection strings, JWT signing keys, PFX files, SMS credentials, or payment-provider secrets to this public repository.
- The current public website is not proof of a complete Linux port. The recovered C# is decompiled from published assemblies and must pass clean build and runtime tests.
- Do not declare success based only on HTTP 200. Every page must load its assets and its API calls must have expected status/data.
- Do not run real SMS, payment, bank inquiry, or recurring financial jobs during migration tests.

## Confirmed starting point

- Repository: digibile/vamcity.
- Legacy app: ASP.NET Core originally targeting .NET 5; recovered source currently targets .NET 9.
- The recovered project uses Microsoft.EntityFrameworkCore.SqlServer, UseSqlServer, SQL Server migrations, and Hangfire.SqlServer. These can remain in a Linux deployment when the SQL Server endpoint is itself Linux-compatible; a database-provider migration is a separate project and is not required merely to run ASP.NET Core on Linux.
- The restored database backup was previously verified on the server as isolated vamcity_stage; prior notes recorded 83 tables at restore verification. This is not proof that every row, attachment, or financial history is present in the target Linux database.
- The application has platform-sensitive components including DinkToPdf/native libwkhtmltox, System.Drawing, file uploads, certificate handling, scheduled tasks, and external gateways.
- A PostgreSQL Compose foundation exists, but it must not be used as a drop-in replacement for SQL Server. Preserve the current provider unless a separately approved, tested data/provider migration is explicitly required.

## Execution phases and acceptance gates

### 1. Preserve and inventory
- Record checksums for the original Windows archive and SQL Server backup without modifying either.
- Inventory all tracked and archive files, configuration keys (names only), static assets, native libraries, certificates, and database tables.
- Keep private artifacts off public GitHub and out of Actions artifacts.

### 2. Recover and compile source
- Treat recovered-source/ as a decompilation baseline, not guaranteed original source.
- Build from a clean Linux runner and resolve compile/package/runtime errors rather than copying Windows publish output.
- Keep all recovered business entities, controllers, authorization rules, and static assets under version control.

### 3. Make schema changes explicit
- Automatic EF migrations and seed initialization are now opt-in in the migration branch via VAMCITY_AUTO_MIGRATE=true.
- Leave this variable unset/false for normal startup and route testing.
- Only enable it after an approved backup, a reviewed migration script, and confirmation that the connection points to the isolated test database.

### 4. Database and background jobs
- First prove that the existing SQL Server database can be reached from the Linux app using the supported SQL client and that the current schema and migrations work unchanged.
- Keep SQL Server and Hangfire.SqlServer when they are hosted on a supported Linux SQL Server service; do not force a PostgreSQL conversion solely because the application host changed from Windows to Linux.
- If a separate move to PostgreSQL is explicitly approved, treat it as its own migration: audit SQL Server-only types, raw SQL, functions, stored procedures, identity/sequence behavior, collation, indexes, constraints, and migration annotations; then implement and test a provider-specific EF Core model and Hangfire storage conversion.
- For any database migration, use a repeatable, idempotent import. Compare table-by-table row counts, key relationships, attachments, identity values, and financial totals before/after. Never treat an empty database as a migration.
- If a database behavior cannot be translated safely, preserve the source data and report the blocker rather than silently dropping behavior.

### 5. Linux runtime compatibility
- Verify Linux x64 native PDF dependencies and required system libraries; never ship Windows .dll native libraries as Linux dependencies.
- Replace unsupported server-side System.Drawing usage where present.
- Validate case-sensitive paths, file permissions, uploads, certificate access, logs, time zone/date handling, and persistent storage.
- Ensure background jobs and all SMS/payment/bank integrations are disabled in test mode.

### 6. Route and browser parity
- Extract all SPA routes, navigation links, API endpoint strings, static asset references, and role-protected pages from recovered source and the compiled bundle.
- Exercise public pages, login modes, registration, plan list/details, customer requests, admin loan requests, loan management, reports, uploads/downloads, and role-based access in desktop and mobile browsers.
- Record all 3xx/401/403/404/5xx responses and browser console errors. A 401/403 may be correct for anonymous/unauthorized access, but must be the expected JSON response and not a broken blank page.
- Verify every linked route, form submission, and API response using non-production test accounts/data.

### 7. Deployment and rollback
- Publish to a separate Linux staging release and port; do not switch the active domain until all gates pass.
- Verify health checks, logs, TLS, file persistence, DB integrity, and rollback to the prior release.
- Only after written evidence for all gates should the production cutover be considered.

## Current status

This branch has a successful .NET 9 Linux publish and a passing source-compatibility inventory on its current baseline. The publish workflow was strengthened to flatten the SPA entrypoint and assert the root HTML, JavaScript bundles, and theme CSS exist in the published web root. A new isolated runtime smoke test is being exercised against an ephemeral SQL Server; it must pass before its claims are accepted. No production deployment or real-data restore is performed by these CI tests.

The migration is not yet production-approved. Outstanding gates include the live Linux runtime test, PDF/native dependency validation, full route/API browser coverage, and row-by-row integrity comparison for any real database copy. The current SQL Server provider is retained by default to reduce data risk; PostgreSQL conversion is not assumed or required just because the app runs on Linux.
