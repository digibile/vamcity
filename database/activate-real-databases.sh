#!/usr/bin/env bash
# Activate the verified legacy VamCity data on the Linux app, with an automatic rollback container.
# Database archives stay on the server; this script never uploads customer data to GitHub.
set -Eeuo pipefail
umask 077

ROOT="/home/digibail/domains/digebail.ir"
APP="vamcity-stage-app"
SQLC="vamcity-stage-sql"
STAGE_DB="vamcity_stage"
SOURCE_DB="vamcityc_vamcity_import_20261028"
BLU_DB="vamcityc_bluvamdb_import_20261028"
TRANSFER_DB="vamcityc_transferDb_import_20261028"
RELEASE="/home/roofixo/domains/roofixo.ir/vamcity-app/releases/3d2d5546cd79fa77e3807eb5a1a5ea8379ecbbfe"
UPLOADS="/home/roofixo/domains/roofixo.ir/vamcity-app/private/uploads"
NETWORK="vamcity-stage-net"
STAMP="$(date +%Y%m%d_%H%M%S)"
CANDIDATE_DB="vamcityc_vamcity_live_"$STAMP
SEED_BAK="vamcity_seed_"$STAMP".bak"
STAGE_BAK="vamcity_stage_before_"$STAMP".bak"
CUTOVER_DIR="$HOME/.vamcity-cutover-"$STAMP
OLD_APP="$APP-before-"$STAMP
CUTOVER_STARTED=0
SQLCMD=""
PASS=""

rollback() {
  rc=$?
  if [ "$rc" -ne 0 ] && [ "$CUTOVER_STARTED" -eq 1 ]; then
    echo "CUTOVER_FAILED=1"
    set +e
    if docker inspect "$OLD_APP" >/dev/null 2>&1; then
      docker rm -f "$APP" >/dev/null 2>&1
      docker rename "$OLD_APP" "$APP" >/dev/null 2>&1
      docker start "$APP" >/dev/null 2>&1
      echo "AUTOMATIC_ROLLBACK=RESTORED_OLD_CONTAINER"
    elif docker inspect "$APP" >/dev/null 2>&1; then
      docker start "$APP" >/dev/null 2>&1
      echo "AUTOMATIC_ROLLBACK=STARTED_EXISTING_CONTAINER"
    else
      echo "AUTOMATIC_ROLLBACK_FAILED=NO_APP_CONTAINER_FOUND"
    fi
  fi
  exit "$rc"
}
trap rollback EXIT

echo "=== PREFLIGHT ==="
hostname
date -Is
[ -d "$ROOT" ] || { echo "ERROR=backup directory not found"; exit 10; }
[ -w "$ROOT" ] || { echo "ERROR=backup directory is not writable by deployment user"; exit 11; }
docker inspect "$APP" >/dev/null
docker inspect "$SQLC" >/dev/null
docker network inspect "$NETWORK" >/dev/null
[ -f "$RELEASE/LoanSystemAvizhe.Site.dll" ] || { echo "ERROR=active release DLL not found"; exit 12; }
[ -d "$UPLOADS" ] || { echo "ERROR=uploads directory not found"; exit 13; }
for f in \
  "$ROOT/vamcityc_bluvamdb_2026-10-29_19-19-45.zip" \
  "$ROOT/vamcityc_transferDb_2026-10-29_19-19-51.zip" \
  "$ROOT/vamcityc_vamcity_2026-10-29_19-20-04.zip"; do
  [ -f "$f" ] || { echo "ERROR=missing archive $(basename "$f")"; exit 14; }
  unzip -t "$f" >/dev/null || { echo "ERROR=archive test failed $(basename "$f")"; exit 15; }
  echo "ARCHIVE_OK=$(basename "$f")"
done

PASS="$(docker inspect "$SQLC" --format '{{range .Config.Env}}{{println .}}{{end}}' | sed -n 's/^MSSQL_SA_PASSWORD=//p;s/^SA_PASSWORD=//p' | head -n 1)"
[ -n "$PASS" ] || { echo "ERROR=SQL Server credential not available from container environment"; exit 16; }
SQLCMD="$(docker exec "$SQLC" sh -lc 'command -v sqlcmd || test ! -x /opt/mssql-tools18/bin/sqlcmd || echo /opt/mssql-tools18/bin/sqlcmd' 2>/dev/null | head -n 1)"
[ -n "$SQLCMD" ] || SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
sql() {
  docker exec -e SQLCMDPASSWORD="$PASS" "$SQLC" "$SQLCMD" -S localhost -U sa -C -b -W -s '|' -Q "$1"
}
for db in "$SOURCE_DB" "$BLU_DB" "$TRANSFER_DB" "$STAGE_DB"; do
  state="$(sql "SET NOCOUNT ON; SELECT state_desc FROM sys.databases WHERE name=N'$db'")"
  echo "$state" | grep -q 'ONLINE' || { echo "ERROR=database not ONLINE: $db"; exit 17; }
  echo "DATABASE_ONLINE=$db"
done

echo "=== PRESERVE CURRENT STAGE DATABASE ==="
sql "BACKUP DATABASE [$STAGE_DB] TO DISK=N'/var/opt/mssql/data/$STAGE_BAK' WITH COPY_ONLY, INIT, COMPRESSION, STATS=10"
docker cp "$SQLC:/var/opt/mssql/data/$STAGE_BAK" "$ROOT/$STAGE_BAK" >/dev/null
chmod 600 "$ROOT/$STAGE_BAK"
sha256sum "$ROOT/$STAGE_BAK"
docker exec -u 0 "$SQLC" rm -f "/var/opt/mssql/data/$STAGE_BAK"

echo "=== CREATE ISOLATED LIVE DATABASE FROM VERIFIED IMPORT ==="
EXISTING="$(sql "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name=N'$CANDIDATE_DB'")"
[ -z "$EXISTING" ] || { echo "ERROR=candidate database name already exists"; exit 18; }
DATA_PATH="/var/opt/mssql/data/"$CANDIDATE_DB"_data.mdf"
LOG_PATH="/var/opt/mssql/data/"$CANDIDATE_DB"_log.ldf"
sql "BACKUP DATABASE [$SOURCE_DB] TO DISK=N'/var/opt/mssql/data/$SEED_BAK' WITH COPY_ONLY, INIT, COMPRESSION, STATS=10"
sql "RESTORE VERIFYONLY FROM DISK=N'/var/opt/mssql/data/$SEED_BAK'"
sql "RESTORE DATABASE [$CANDIDATE_DB] FROM DISK=N'/var/opt/mssql/data/$SEED_BAK' WITH MOVE N'vamciydb' TO N'$DATA_PATH', MOVE N'vamciydb_log' TO N'$LOG_PATH', RECOVERY, STATS=10"
docker exec -u 0 "$SQLC" rm -f "/var/opt/mssql/data/$SEED_BAK"
sql "DBCC CHECKDB (N'$CANDIDATE_DB') WITH NO_INFOMSGS"

echo "=== ALIGN THE SINGLE COLUMN REQUIRED BY THE CURRENT APP ==="
sql "IF NOT EXISTS (SELECT 1 FROM [$CANDIDATE_DB].sys.columns c JOIN [$CANDIDATE_DB].sys.tables t ON t.object_id=c.object_id WHERE t.name=N'UserLoanRequestAuthorizationItems' AND c.name=N'ThirdPersonAssuranceAuthorizationItemId') ALTER TABLE [$CANDIDATE_DB].dbo.UserLoanRequestAuthorizationItems ADD ThirdPersonAssuranceAuthorizationItemId uniqueidentifier NULL"

echo "=== PRESERVE THE EXISTING STAGE SUPERADMIN LOGIN ==="
MERGE_SQL="$(cat <<SQL
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
  BEGIN TRANSACTION;
  DECLARE @sourceUserId nvarchar(450), @targetUserId nvarchar(450),
          @sourceNormUserName nvarchar(256), @sourceNormEmail nvarchar(256),
          @cols nvarchar(max), @srcCols nvarchar(max), @dyn nvarchar(max),
          @status nvarchar(40), @adminRoleId nvarchar(450), @superRoleId nvarchar(450);

  SELECT TOP (1) @sourceUserId=ur.UserId
  FROM [$STAGE_DB].dbo.AspNetUserRoles ur
  JOIN [$STAGE_DB].dbo.AspNetRoles r ON r.Id=ur.RoleId
  WHERE r.Name=N'SuperAdmin'
  ORDER BY ur.UserId;

  IF @sourceUserId IS NULL
    THROW 51001, 'Stage SuperAdmin account not found; refusing database cutover.', 1;

  SELECT @sourceNormUserName=NormalizedUserName, @sourceNormEmail=NormalizedEmail
  FROM [$STAGE_DB].dbo.AspNetUsers WHERE Id=@sourceUserId;

  SELECT TOP (1) @targetUserId=Id
  FROM [$CANDIDATE_DB].dbo.AspNetUsers WHERE Id=@sourceUserId;

  IF @targetUserId IS NULL AND @sourceNormUserName IS NOT NULL
    SELECT TOP (1) @targetUserId=Id
    FROM [$CANDIDATE_DB].dbo.AspNetUsers WHERE NormalizedUserName=@sourceNormUserName;

  IF @targetUserId IS NULL AND @sourceNormEmail IS NOT NULL
     AND EXISTS (SELECT 1 FROM [$CANDIDATE_DB].dbo.AspNetUsers WHERE NormalizedEmail=@sourceNormEmail)
    THROW 51002, 'SuperAdmin email conflicts with a different legacy user; refusing automatic merge.', 1;

  IF @targetUserId IS NULL
  BEGIN
    SELECT
      @cols=STRING_AGG(QUOTENAME(sc.name),N',') WITHIN GROUP (ORDER BY sc.column_id),
      @srcCols=STRING_AGG(N's.'+QUOTENAME(sc.name),N',') WITHIN GROUP (ORDER BY sc.column_id)
    FROM [$STAGE_DB].sys.columns sc
    JOIN [$CANDIDATE_DB].sys.columns tc ON tc.name=sc.name
    WHERE sc.object_id=(SELECT object_id FROM [$STAGE_DB].sys.tables WHERE name=N'AspNetUsers')
      AND tc.object_id=(SELECT object_id FROM [$CANDIDATE_DB].sys.tables WHERE name=N'AspNetUsers')
      AND sc.is_identity=0 AND sc.is_computed=0 AND tc.is_identity=0 AND tc.is_computed=0;
    IF NULLIF(@cols,N'') IS NULL OR NULLIF(@srcCols,N'') IS NULL
      THROW 51003, 'Could not safely map AspNetUsers columns.', 1;
    SET @dyn=N'INSERT INTO [$CANDIDATE_DB].dbo.AspNetUsers ('+@cols+N') SELECT '+@srcCols+
             N' FROM [$STAGE_DB].dbo.AspNetUsers s WHERE s.Id=@sourceId';
    EXEC sp_executesql @dyn,N'@sourceId nvarchar(450)',@sourceId=@sourceUserId;
    SET @targetUserId=@sourceUserId;
    SET @status=N'SUPERADMIN_COPIED';
  END
  ELSE
  BEGIN
    UPDATE t
      SET t.PasswordHash=s.PasswordHash,
          t.SecurityStamp=s.SecurityStamp,
          t.ConcurrencyStamp=s.ConcurrencyStamp,
          t.LockoutEnd=NULL,
          t.AccessFailedCount=0
    FROM [$CANDIDATE_DB].dbo.AspNetUsers t
    JOIN [$STAGE_DB].dbo.AspNetUsers s ON s.Id=@sourceUserId
    WHERE t.Id=@targetUserId;
    SET @status=N'SUPERADMIN_MATCHED';
  END;

  SELECT @adminRoleId=Id FROM [$CANDIDATE_DB].dbo.AspNetRoles WHERE Name=N'Admin';
  SELECT @superRoleId=Id FROM [$CANDIDATE_DB].dbo.AspNetRoles WHERE Name=N'SuperAdmin';
  IF @adminRoleId IS NULL OR @superRoleId IS NULL
    THROW 51004, 'Required Admin/SuperAdmin role is missing in candidate database.', 1;

  INSERT INTO [$CANDIDATE_DB].dbo.AspNetUserRoles(UserId,RoleId)
  SELECT @targetUserId,r.Id
  FROM [$CANDIDATE_DB].dbo.AspNetRoles r
  WHERE r.Name IN (N'Admin',N'SuperAdmin')
    AND NOT EXISTS (SELECT 1 FROM [$CANDIDATE_DB].dbo.AspNetUserRoles ur WHERE ur.UserId=@targetUserId AND ur.RoleId=r.Id);

  IF OBJECT_ID(N'[$CANDIDATE_DB].dbo.AspNetUserClaims',N'U') IS NOT NULL
    INSERT INTO [$CANDIDATE_DB].dbo.AspNetUserClaims(UserId,ClaimType,ClaimValue)
    SELECT @targetUserId,c.ClaimType,c.ClaimValue
    FROM [$STAGE_DB].dbo.AspNetUserClaims c
    WHERE c.UserId=@sourceUserId
      AND NOT EXISTS (
        SELECT 1 FROM [$CANDIDATE_DB].dbo.AspNetUserClaims t
        WHERE t.UserId=@targetUserId
          AND ISNULL(t.ClaimType,N'')=ISNULL(c.ClaimType,N'')
          AND ISNULL(t.ClaimValue,N'')=ISNULL(c.ClaimValue,N'')
      );

  COMMIT TRANSACTION;
  SELECT @status AS superadmin_merge_status,
         (SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.AspNetUsers) AS users_after_merge,
         (SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.AspNetUserRoles ur JOIN [$CANDIDATE_DB].dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE r.Name=N'SuperAdmin') AS superadmin_assignments;
END TRY
BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
SQL
)"
sql "$MERGE_SQL"

echo "=== VERIFY CANDIDATE DATA ==="
sql "SET NOCOUNT ON; SELECT N'$CANDIDATE_DB' AS database_name,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.AspNetUsers) AS users,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.UserLoanRequests) AS loan_requests,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.UserLoanPayments) AS payments,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.Loans) AS loans,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.Customers) AS customers"
for db in "$BLU_DB" "$TRANSFER_DB"; do
  sql "SET NOCOUNT ON; SELECT DB_NAME(database_id) AS database_name,state_desc FROM sys.databases WHERE name=N'$db'"
done

echo "=== SAVE ROLLBACK CONFIGURATION ==="
mkdir -m 700 -p "$CUTOVER_DIR"
docker inspect "$APP" --format '{{range .Config.Env}}{{println .}}{{end}}' > "$CUTOVER_DIR/old.env"
chmod 600 "$CUTOVER_DIR/old.env"
cp "$CUTOVER_DIR/old.env" "$CUTOVER_DIR/new.env"
python3 - "$CUTOVER_DIR/new.env" "$CANDIDATE_DB" "$BLU_DB" "$TRANSFER_DB" <<'PY'
import re,sys
path,target,blu,transfer=sys.argv[1:]
lines=open(path,encoding="utf-8").read().splitlines()
out=[]
changed=False
for line in lines:
    if line.startswith("ConnectionStrings__DefaultConnection="):
        line=re.sub(r"(?i)(Database=)[^;]+",lambda m:m.group(1)+target,line,count=1)
        changed=True
    elif line.startswith("ConnectionStrings__") and "=" in line:
        key,val=line.split("=",1)
        val=re.sub(r"(?i)(Database=)vamcityc_bluvamdb(?:_import_20261028)?(?=;|$)",lambda m:m.group(1)+blu,val)
        val=re.sub(r"(?i)(Database=)vamcityc_transferDb(?:_import_20261028)?(?=;|$)",lambda m:m.group(1)+transfer,val)
        line=key+"="+val
    out.append(line)
if not changed:
    raise SystemExit("DefaultConnection environment variable not found; app was not changed.")
open(path,"w",encoding="utf-8").write("\n".join(out)+"\n")
PY
chmod 600 "$CUTOVER_DIR/new.env"

echo "=== SWITCH APPLICATION CONNECTION WITH AUTOMATIC ROLLBACK ==="
CUTOVER_STARTED=1
docker stop "$APP" >/dev/null
docker rename "$APP" "$OLD_APP"
docker run -d \
  --name "$APP" \
  --restart unless-stopped \
  --network "$NETWORK" \
  -p 127.0.0.1:5187:8080 \
  --mount "type=bind,src=$RELEASE,dst=/app" \
  --mount "type=bind,src=$UPLOADS,dst=/app/uploads" \
  --env-file "$CUTOVER_DIR/new.env" \
  --workdir /app \
  mcr.microsoft.com/dotnet/aspnet:10.0 \
  LoanSystemAvizhe.Site.dll

sleep 10
ROOT_HTTP="$(curl --max-time 5 -sS -o /dev/null -w '%{http_code}' http://127.0.0.1:5187/ || echo 000)"
ADMIN_HTTP="$(curl --max-time 5 -sS -o /dev/null -w '%{http_code}' http://127.0.0.1:5187/admin || echo 000)"
LOGIN_HTTP="$(curl --max-time 5 -sS -o /dev/null -w '%{http_code}' http://127.0.0.1:5187/api/v1/security/captcha || echo 000)"
echo "ROOT_HTTP=$ROOT_HTTP"
echo "ADMIN_HTTP=$ADMIN_HTTP"
echo "LOGIN_CAPTCHA_HTTP=$LOGIN_HTTP"
[ "$ROOT_HTTP" = "200" ] && [ "$ADMIN_HTTP" = "200" ] && [ "$LOGIN_HTTP" = "200" ] || { echo "ERROR=application smoke test failed"; exit 30; }

CRITICAL="$(docker logs --since 2m "$APP" 2>&1 | grep -Ei 'Invalid column name|Invalid object name|Cannot open database|Login failed for user|Unhandled exception|SqlException|applied to the database but are not present in the current assembly' | tail -n 20 || true)"
if [ -n "$CRITICAL" ]; then
  echo "ERROR=database/runtime exception detected after cutover"
  printf '%s\n' "$CRITICAL" | sed -E 's/(Password|Pwd|Token|Secret)=([^; ]+)/\1=[REDACTED]/Ig'
  exit 31
fi

echo "=== FINAL DATABASE COUNTS ==="
sql "SET NOCOUNT ON; SELECT N'$CANDIDATE_DB' AS database_name,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.AspNetUsers) AS users,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.UserLoanRequests) AS loan_requests,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.UserLoanPayments) AS payments,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.Loans) AS loans,(SELECT COUNT(*) FROM [$CANDIDATE_DB].dbo.Customers) AS customers"
echo "ACTIVE_DATABASE=$CANDIDATE_DB"
echo "SUPERADMIN_CREDENTIALS_PRESERVED=1"
echo "ROLLBACK_CONTAINER=$OLD_APP"
echo "STAGE_BACKUP=$ROOT/$STAGE_BAK"
echo "ROLLBACK_FILES=$CUTOVER_DIR"
echo "VAMCITY_REAL_DATABASE_ACTIVATION=SUCCESS"
CUTOVER_STARTED=0
trap - EXIT
