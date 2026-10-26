# Ghestine Platform — Snapshot from digibail.ir

This directory is reserved for a separate snapshot of the Ghestine platform currently served from `digibail.ir`.

## Scope

The server inspection identified these candidate files/pages:

- `/home/digibail/domains/digibail.ir/public_html/index.html` — main Ghestine page
- `/home/digibail/domains/digibail.ir/public_html/admin-preview/index.html` — admin preview
- `/home/digibail/domains/digibail.ir/public_html/beta/index.html` — beta-plan management page
- `/home/digibail/domains/digibail.ir/stats/index.html` — stats page (outside public_html)

Potential related archives were also seen in `public_html`: `ghestine-beta-full.zip`, `ghestine-core _1_.zip`, and `ghestine-full _1_.zip`. Their contents have **not** yet been inspected or imported.

## Important status

This README is a staging marker only. It does **not** mean the live platform source has already been transferred. The server files still need to be copied into this directory, reviewed, and checked for secrets before committing.

## Safe import checklist

1. Copy the live files into this directory without changing the running site.
2. Inspect archive contents and compare them with the live pages to determine which source is authoritative.
3. Exclude secrets, credentials, private keys, production environment files, database dumps, user data, and uploaded customer documents.
4. Add a short inventory of the imported files and how to run/preview them.
5. Commit the reviewed snapshot on a dedicated branch before any deployment work.

No production deployment or server-side changes are performed by this directory.
