# VamCity Linux Migration

## Verified status (2026-10-12)

- The uploaded `Publish.rar` and `Publish1.rar` archives are readable with `unar`; the server's installed 7-Zip 21.07 could not open them.
- The uploaded publish package is an old ASP.NET Core/.NET 5 deployment package. .NET 5 is end-of-life and must not be the production runtime target.
- The package contains both Windows-native assets and a Linux x64 `libwkhtmltox.so`. The presence of that shared library alone does not make the whole package production-ready for Linux.
- Recovered C# source and Entity Framework migration files are present under `recovered-source/`.
- The recovered web project targets .NET 9. A Linux CI publish completed successfully on 2026-10-12 and produced the `vamcity-linux-publish` artifact (workflow run 38223448346).
- A Linux staging deployment workflow also completed successfully (workflow run 38223448334). Separate browser verification later failed on a legacy plans-route redirect assertion; therefore, the site is not considered fully functionally verified yet.
- Do not deploy the old archive as the final Linux release. Build from `recovered-source/`, publish for Linux, and validate all platform-specific dependencies.

## Migration steps

1. Treat the original Windows publish archives as forensic/reference input only; preserve them unchanged.
2. Build from the recovered solution/project files rather than running the old .NET 5 publish output.
3. Keep the application on a supported .NET LTS version and run a clean Linux CI publish on every source change.
4. Resolve Linux compatibility for file paths, certificates, PDF generation, background jobs, and database access. Confirm the native `libwkhtmltox.so` architecture and all required system libraries on the target server.
5. Keep SQL Server database compatibility explicit: the current application references Entity Framework SQL Server and Hangfire SQL Server. Do not assume a PostgreSQL migration has happened unless the provider, schema, and data migration are actually implemented and tested.
6. Move connection strings, JWT keys, payment/SMS credentials, and certificates into server-side secrets. Never commit production database backups or private PFX/P12 certificates.
7. Keep payment, SMS, banking, and recurring jobs disabled during isolated tests.
8. Deploy only after Linux build, application startup, login, plan pages, form submissions, background-job isolation, and smoke tests pass. Use a non-root service account, reverse proxy, health checks, logs, and rollback.
9. Keep VamCity separate from the NAF/Sookar deployment.

## Safety requirements

- Never commit real credentials, appsettings files containing secrets, or private PFX/P12 certificates.
- Never run migration tests against production data.
- Do not mark migration complete until a clean Linux build and functional tests pass.

## Package audit

Run `bash scripts/audit-package.sh .` to inspect source availability, runtime target, and potentially sensitive files.
