# VamCity Linux / PostgreSQL staging foundation

This directory provides an **isolated PostgreSQL 16 service** for migration testing. It does not replace the production database and does not make the legacy application PostgreSQL-compatible by itself.

## Start the isolated database

From this directory on a Linux host with Docker Compose:

1. Copy `.env.example` to `.env` and set a unique, long random password.
2. Restrict the file: `chmod 600 .env`.
3. Start it: `docker compose up -d`.
4. Check readiness: `docker compose ps` and `docker compose logs --tail=100 vamcity-postgres`.

The database is bound to loopback port 55432, not exposed on all network interfaces. Do not reuse another application's database or volume.

## Application migration status

The recovered legacy application is not yet ready to use this database. The current code uses:

- Entity Framework Core SQL Server provider and `UseSqlServer` in startup.
- SQL Server-specific EF Core migrations and schema annotations.
- Hangfire SQL Server storage.
- SQL Server client libraries and potentially provider-specific SQL in application code.

Do **not** point the application at PostgreSQL until all provider-specific code has been migrated, a new PostgreSQL migration chain has been generated from the actual model, and integration tests pass. Existing SQL Server migrations must not be applied to PostgreSQL unchanged.

## Required migration sequence

1. Inventory SQL Server-only APIs, raw SQL, stored procedures, identity/sequence behavior, computed columns, indexes, and data types.
2. Add a PostgreSQL EF Core provider and convert query/DDL assumptions; preserve SQL Server support only for a controlled data-export tool if needed.
3. Replace Hangfire SQL Server storage with a compatible PostgreSQL storage provider and test recurring/background jobs.
4. Generate a fresh PostgreSQL baseline migration from the complete current model. Review constraints, indexes, relationships, and Identity tables.
5. Apply migrations to this isolated database and run integration tests for authentication, customers/tenants, loan requests, payments, attachments, and background jobs.
6. Export source data from the original database and build a repeatable, idempotent ETL/import with row counts and integrity checks. Never treat an empty schema as a data transfer.
7. Deploy to a separate staging path and port on the Linux server; do not change the active Roofixo site until smoke tests and rollback are verified.

## Secrets and production safety

- Keep `.env`, database backups, PFX certificates, API keys, and JWT secrets out of Git.
- Use environment variables such as `ConnectionStrings__DefaultConnection` and `AppConfig__JwtSecretKey` for the application.
- Keep payment, SMS, email, and scheduled external side effects disabled in migration tests until explicitly configured.
- Back up any target database before schema changes. Never run experimental migrations against production data.
