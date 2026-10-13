# VamCity Linux Migration Audit (evidence-based, 2026-10-13)

## Scope and confidence

This report is based on files readable in `digibile/vamcity` through GitHub. It is not a claim that the server filesystem or the 1.97 GB archive has been read directly. The archive currently exists at a server path reported by the operator, but GitHub access does not grant shell access to that server.

## Confirmed repository findings

- The repository contains a published deployment package (commit `04b6fd5`, reported as 1,125 changed files) including `LoanSystemAvizhe.Site.dll`, related assemblies, `.deps.json`, `.runtimeconfig.json`, Windows `.exe`, and static assets.
- `recovered-source/README.md` explicitly says this is C# decompiled from published assemblies with ILSpy, not yet a validated, buildable Linux port.
- Recovered project files target `net9.0`. The recovered README says the original application targeted .NET 5.
- The Linux workflow `.github/workflows/linux-build.yml` attempts a .NET 9 publish and uploads `vamcity-linux-publish`; the existence of this workflow is not proof that the workflow passed.
- The source project references SQL Server / EF Core SQL Server and Hangfire SQL Server. No migration to PostgreSQL is established by these project files.
- The services project has assembly-file HintPath dependencies including `System.Drawing.Common.dll`, `DinkToPdf.dll`, and legacy assemblies; these need dependency-by-dependency validation and replacement/configuration where native Windows binaries are used.
- The recovered source README itself flags authentication, configuration, file paths, PDF generation, and external integrations for validation.
- `Startup.cs` reads `AppConfig:JwtSecretKey` and throws if it is absent or shorter than 32 characters. Provide it through secure environment configuration, not source control.
- A recent commit changes legacy loan-plan URL matching and redirects to `/cl/plans/`; this is a route correction, not proof that member/loan admin pages or APIs are healthy.

## Windows-specific / native dependency review

1. `runtimes/win-x64/native/libwkhtmltox.dll` (observed in the server archive listing): Windows native PDF library. It cannot be loaded as a Linux native library. If DinkToPdf is used, supply a compatible Linux `libwkhtmltox.so` build with its required system libraries, or replace PDF generation with a cross-platform implementation. Validate actual call sites and licensing before choosing.
2. `runtimes/win-*/native/Microsoft.Data.SqlClient.SNI.dll` and `sni.dll`: Windows native SQL client components. Linux requires the package's Linux-native assets; do not copy Windows SNI binaries into the Linux publish output. Restore supported NuGet packages rather than relying on arbitrary copied DLLs.
3. `System.Drawing.Common.dll`: risky for server-side image processing on Linux, depending on target/package/version and usage. Locate call sites and migrate to a supported cross-platform library such as ImageSharp or SkiaSharp if needed.
4. `LoanSystemAvizhe.Site.exe` and `web.config`: Windows/IIS deployment artifacts. Linux startup should use `dotnet LoanSystemAvizhe.Site.dll` or a Linux apphost produced by `dotnet publish -r linux-x64`; IIS `web.config` is not the Linux process manager.
5. `.pfx` certificate: secret/private-key material. Keep it out of Git and public web roots; provision securely with restrictive permissions and rotate if it has been exposed.
6. Windows runtime folders may include unrelated tooling/native assets. Presence alone is not proof they are loaded. Trace runtime call sites and inspect the final Linux publish directory before removing or replacing anything.

## Data integrity: not yet confirmed

The repository files and static UI cannot establish whether all loan plans, applications, representatives/agents, users, permissions/submenus, and historical financial records exist in the database backup. That requires reading the actual backup and comparing table names/counts with the live/staging database. Do not overwrite the current database during this audit.

## Required gates before deployment

- Recover the actual original source if available; treat decompiled source as a recovery baseline and compare it against assemblies.
- Run the Linux build workflow and capture the actual success/failure logs.
- Replace absolute/Windows paths and any Windows-only native dependency.
- Verify Linux PDF generation, image processing, SQL connection, Hangfire jobs, certificate loading, SMS/payment integrations, uploads, and logging in staging.
- Test login/authentication and all admin routes plus their Fetch/XHR API responses (401/403/404/500) in a browser.
- Validate database backup integrity and record counts before any migration or restore.
- Keep production data and running containers untouched until the staging build and smoke tests pass.
