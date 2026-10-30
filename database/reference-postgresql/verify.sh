#!/usr/bin/env sh
set -eu

CONTAINER="${POSTGRES_CONTAINER:-vamcity-reference-postgres}"
DATABASE="${POSTGRES_DB:-vamcity_reference}"
DB_USER="${POSTGRES_USER:-vamcity_migrator}"

if ! command -v docker >/dev/null 2>&1; then
  echo "ERROR: docker is required" >&2
  exit 1
fi

docker inspect "$CONTAINER" >/dev/null 2>&1 || {
  echo "ERROR: container not found: $CONTAINER" >&2
  exit 1
}

docker exec "$CONTAINER" pg_isready -U "$DB_USER" -d "$DATABASE"

docker exec -e PGPASSWORD= -i "$CONTAINER" \
  psql -U "$DB_USER" -d "$DATABASE" -v ON_ERROR_STOP=1 -At <<'SQL'
SELECT 'database=' || current_database();
SELECT 'server_version=' || current_setting('server_version');
SELECT 'control_schema=' || EXISTS (
  SELECT 1 FROM information_schema.schemata
  WHERE schema_name = 'migration_control'
);
SELECT 'control_tables=' || count(*)
FROM information_schema.tables
WHERE table_schema = 'migration_control' AND table_type = 'BASE TABLE';
SQL
