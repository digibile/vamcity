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
- The recovered project still uses Microsoft.EntityFrameworkCore.SqlServer, UseSqlServer, SQL Server migrations, and Hangfire.SqlServer.
- The restored database backup was previously verified on the server as isolated vamcity_stage; prior notes recorded 83 tables at restore verification. This is not proof that every row, attachment, or financial history is present in the target Linux database.
- The application has platform-sensitive components including DinkToPdf/native libwkhtmltox, System.Drawing, file uploads, certificate handling, scheduled tasks, and external gateways.
- A PostgreSQL Compose foundation exists, but that alone does not convert the schema, data, queries, or Hangfire storage. Do not connect the current SQL Server application to PostgreSQL.

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

### 4. Convert database and background jobs
- Audit SQL Server-only types, raw SQL, functions, stored procedures, identity/sequence behavior, collation, indexes, constraints, and migration annotations.
- Implement a reviewed PostgreSQL EF Core model/migration baseline before selecting PostgreSQL as the app provider.
- Replace UseSqlServer and Hangfire.SqlServer with compatible PostgreSQL providers only after the model and job-storage conversion exists.
- Import source data with a repeatable, idempotent ETL. Compare table-by-table row counts, key relationships, attachments, identity values, and financial totals before/after. Never treat an empty database as a migration.
- If any SQL Server-specific behavior cannot be translated safely, keep it isolated and report the blocker rather than silently dropping behavior.

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

This branch begins the controlled migration work. It does not claim the SQL Server-to-PostgreSQL conversion, complete data transfer, every-route verification, or final production cutover is finished. The immediate next gates are the clean Linux build, source/runtime dependency audit, and a complete route/API inventory.
