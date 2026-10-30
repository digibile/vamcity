# VamCity reference database (Windows SQL Server -> Linux PostgreSQL)

This is the isolated reference target for the VamCity database migration. It is deliberately separate from the existing staging PostgreSQL service and from every production database.

## What is created

- PostgreSQL 16 database: `vamcity_reference`
- Container: `vamcity-reference-postgres`
- Loopback-only host port: `55433`
- Dedicated Docker volume: `vamcity_reference_pgdata`
- Schema `migration_control` for source inventory, object mapping, migration batches, and validation evidence.

The initialization SQL creates only migration-control tables. It does **not** invent VamCity business tables or pretend that an empty PostgreSQL database is a converted application database.

## Source of truth

The recovered application model is in:

- `recovered-source/LoanSystemAvizhe.Data/LoanSystem.Data/AppDbContext.cs`
- `recovered-source/LoanSystemAvizhe.Data/LoanSystem.Data.Migrations/AppDbContextModelSnapshot.cs`

The snapshot is an EF Core 5.0.2 SQL Server model (127 KB in the source branch), with SQL Server types and annotations such as `uniqueidentifier`, `nvarchar`, `datetime2`, `bit`, `dbo`, and SQL Server identity columns. It is the model reference, not proof that every source database has the same deployed schema.

The recovered application currently configures both EF Core and Hangfire for SQL Server. PostgreSQL is therefore not yet an application-compatible drop-in replacement. The application provider, Hangfire storage, provider-specific SQL, raw SQL, indexes, identity generation, and data types must be converted and tested separately.

## Start the reference database

On a Linux host with Docker Compose:

1. Copy `.env.example` to `.env`.
2. Set a unique random password. Do not commit `.env`.
3. Run `chmod 600 .env`.
4. Run `docker compose up -d` from this directory.
5. Verify with `docker compose ps` and `docker compose logs --tail=100 vamcity-reference-postgres`.

The service binds only to `127.0.0.1:55433`. It has its own volume and does not reuse the existing `vamcity-postgres` volume or the Roofixo production database.

## Required conversion sequence

1. Restore each original Windows SQL Server backup into an isolated SQL Server source instance. Never restore over an active database.
2. Inventory each source independently: tables, columns, SQL types, nullability, defaults, identity/sequence definitions, primary/foreign keys, indexes, views, triggers, procedures, functions, row counts, and EF migration history.
3. Compare the live source inventories with the recovered EF model snapshot. Record differences instead of silently adding or dropping columns.
4. Convert the complete EF model to the Npgsql provider and generate a fresh PostgreSQL baseline migration. Do not apply SQL Server EF migrations to PostgreSQL.
5. Convert Hangfire storage and all SQL Server-specific queries and database routines.
6. Apply the reviewed baseline to this reference database.
7. Import data with deterministic type mapping and preserved primary keys. Validate row counts, key relationships, decimal precision, timestamps, Unicode text, binary/document values, and identity sequences.
8. Run application integration tests against this database before any staging cutover.

## Important type conversions to review

| SQL Server | PostgreSQL target | Review requirement |
|---|---|---|
| `uniqueidentifier` | `uuid` | Preserve all identifiers and FK links |
| `nvarchar(n/max)` | `varchar(n)/text` | Preserve Unicode and length rules |
| `datetime2` | `timestamp without time zone` | Confirm existing timezone semantics |
| `datetimeoffset` | `timestamp with time zone` | Normalize offsets without changing instants |
| `bit` | `boolean` | Verify null/default behavior |
| `decimal(p,s)` | `numeric(p,s)` | Preserve precision and scale |
| `bigint identity` | `bigint generated ... as identity` | Reset sequences after import |
| `int identity` | `integer generated ... as identity` | Reset sequences after import |
| `varbinary(max)` | `bytea` | Verify file/blob storage strategy |
| `nvarchar(max)` JSON payloads | `text` or `jsonb` | Keep as text unless JSON semantics are confirmed |

These are conversion candidates, not blind global replacements. Each column and constraint must be checked against the source definition and application usage.

## Current completion boundary

This repository change creates the isolated PostgreSQL reference database foundation and its migration-control catalog. It does not claim that Windows backup data has been imported, that the business schema baseline has been generated, or that the application has been switched to PostgreSQL. Those steps require the actual source database inventory and provider conversion.
