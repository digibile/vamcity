# VamCity Linux Migration

## Current state

The repository currently contains a published ASP.NET Core package, not the original source solution. The entry assembly targets .NET 5, which is end-of-life.

This repository is the migration workspace. It is not production-ready until the application builds and passes isolated Linux tests.

## Migration steps

1. Recover the original solution and project files (.sln, .csproj, C#, and Razor source).
2. If the source cannot be recovered, use assembly decompilation as a recovery aid. Decompiled code is not the original source and requires reconstruction and review.
3. Move the application to a supported .NET LTS version.
4. Resolve Linux compatibility for paths, certificates, PDF generation, background jobs, and database access.
5. Move connection strings, JWT keys, payment/SMS credentials, and certificates into server-side secrets.
6. Add Linux CI builds and isolated database/integration tests.
7. Deploy only after smoke tests pass, with a non-root service account, reverse proxy, health checks, logs, and rollback.

## Safety requirements

- Never commit real credentials, appsettings files containing secrets, or private PFX/P12 certificates.
- Disable payment, SMS, banking, and recurring jobs during initial tests.
- Never run migration tests against production data.
- Keep VamCity separate from the NAF/Sookar deployment.
- Do not mark migration complete until a clean Linux build and functional tests pass.

## Package audit

Run `bash scripts/audit-package.sh .` to inspect source availability, runtime target, and potentially sensitive files.
