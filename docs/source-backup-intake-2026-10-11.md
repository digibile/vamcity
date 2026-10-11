# VamCity source backup intake

Status recorded: 2026-10-11

## Source files verified

- Database archive on the Roofixo host: `vamcityc_aqayevamdb_2026-10-11_15-51-44.zip`
- ZIP integrity check passed.
- The archive contains a Microsoft SQL Server native backup, approximately 17.9 MB uncompressed.
- The application repository already contains the .NET application binary, recovered C# source and Entity Framework migration files.

## Staging restore

- SQL Server staging database: `vamcity_stage`
- The supplied native backup was restored successfully.
- The database reported 83 tables during the restore verification.
- The application container was running and its local HTTP endpoint responded successfully at the time of the last diagnostic.

## Important: database is not committed to Git

The repository `digibile/vamcity` is currently public. A raw production database may contain personal information, account data, authentication material and payment-provider configuration. Therefore the database backup must not be committed to this public repository or attached to a public Actions artifact.

The database remains on the server in the staging SQL Server and in the server's private backup directory. To store a backup in GitHub, first move the repository to private visibility or create a dedicated private repository, then upload the backup through an access-controlled workflow or encrypted artifact. Keep passwords, signing certificates, API keys and connection strings in GitHub Actions secrets or server-side secret files, never in Git.

## Remaining validation

- Verify the exact public site branding and plan data in a browser.
- Confirm administrator login through the site's normal password-reset or credential-management process; do not store an admin password in this repository.
- Confirm all referenced JavaScript/CSS assets, plan detail pages and application forms work end-to-end.
- Do not treat HTTP 200 alone as proof that the site is fully functional.
