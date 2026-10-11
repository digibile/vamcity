# Recovered VamCity source

This directory contains C# source decompiled from the published VamCity assemblies with ILSpy.
It is a recovery baseline, not yet a validated, buildable Linux port.

## Migration status
- The original application targets .NET 5, which is out of support.
- Decompiled projects may contain assembly-file HintPath references rather than complete NuGet package references.
- The application uses SQL Server / EF Core SQL Server and Hangfire.SqlServer.
- Linux compatibility, configuration, authentication, file paths, PDF generation (DinkToPdf), and external integrations still require validation.
- Do not copy production credentials, appsettings files, PFX certificates, or private keys into this directory.
- Keep production databases and services untouched until a separate Linux build passes tests and a staging migration is verified.

Decompiled code may differ from the original source and may require manual correction before compilation.
