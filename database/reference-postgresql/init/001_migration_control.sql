-- Control catalog only. Business tables are generated from the recovered EF model
-- after provider conversion and review; no guessed business schema is created here.
CREATE SCHEMA IF NOT EXISTS migration_control;

CREATE TABLE IF NOT EXISTS migration_control.source_database (
    source_key text PRIMARY KEY,
    source_engine text NOT NULL,
    source_database_name text NOT NULL,
    source_backup_sha256 text,
    source_backup_name text,
    discovered_at timestamptz,
    inventory_status text NOT NULL DEFAULT 'pending'
        CHECK (inventory_status IN ('pending', 'in_progress', 'complete', 'blocked')),
    notes text,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS migration_control.object_mapping (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    source_key text NOT NULL REFERENCES migration_control.source_database(source_key),
    source_schema text NOT NULL,
    source_object text NOT NULL,
    target_schema text NOT NULL,
    target_object text NOT NULL,
    object_kind text NOT NULL
        CHECK (object_kind IN ('table', 'view', 'sequence', 'function', 'procedure', 'index', 'constraint', 'other')),
    conversion_status text NOT NULL DEFAULT 'pending'
        CHECK (conversion_status IN ('pending', 'mapped', 'converted', 'validated', 'blocked')),
    conversion_notes text,
    UNIQUE (source_key, source_schema, source_object, object_kind)
);

CREATE TABLE IF NOT EXISTS migration_control.migration_batch (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    source_key text NOT NULL REFERENCES migration_control.source_database(source_key),
    batch_key text NOT NULL UNIQUE,
    started_at timestamptz NOT NULL DEFAULT now(),
    completed_at timestamptz,
    status text NOT NULL DEFAULT 'planned'
        CHECK (status IN ('planned', 'running', 'completed', 'failed', 'rolled_back')),
    source_row_count bigint,
    target_row_count bigint,
    checksum_manifest jsonb NOT NULL DEFAULT '{}'::jsonb,
    notes text
);

CREATE TABLE IF NOT EXISTS migration_control.validation_result (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    batch_id bigint NOT NULL REFERENCES migration_control.migration_batch(id),
    object_name text NOT NULL,
    validation_name text NOT NULL,
    source_value text,
    target_value text,
    passed boolean NOT NULL,
    details jsonb NOT NULL DEFAULT '{}'::jsonb,
    checked_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_object_mapping_status
    ON migration_control.object_mapping (conversion_status, source_key);
CREATE INDEX IF NOT EXISTS ix_validation_batch_passed
    ON migration_control.validation_result (batch_id, passed);
