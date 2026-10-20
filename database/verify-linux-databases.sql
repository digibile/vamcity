/*
  VamCity - Linux SQL Server database verification
  Repository: digibile/vamcity
  Branch: database-linux-migration

  Read-only verification only. This script does not create, alter, drop, or restore
  databases and does not contain credentials.
*/
SET NOCOUNT ON;

SELECT
    d.name AS DatabaseName,
    d.state_desc AS DatabaseState,
    d.compatibility_level AS CompatibilityLevel,
    d.recovery_model_desc AS RecoveryModel,
    CASE WHEN d.database_id IS NULL THEN N'MISSING' ELSE N'FOUND' END AS Presence
FROM (VALUES
    (N'vamcityc_vamcity'),
    (N'vamcityc_bluvamdb'),
    (N'vamcityc_transferDb'),
    (N'vamcity_stage')
) AS expected(name)
LEFT JOIN sys.databases AS d
    ON d.name = expected.name
ORDER BY expected.name;

-- EF Core migration history, where present.
DECLARE @db sysname;
DECLARE @sql nvarchar(max);

DECLARE db_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT name
FROM sys.databases
WHERE name IN (N'vamcityc_vamcity', N'vamcityc_bluvamdb', N'vamcityc_transferDb', N'vamcity_stage')
  AND state_desc = N'ONLINE';

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @db;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'IF EXISTS (SELECT 1 FROM ' + QUOTENAME(@db) + N'.sys.tables WHERE name = N''__EFMigrationsHistory'')
    BEGIN
        SELECT N''' + REPLACE(@db,'''','''''') + N''' AS DatabaseName,
               MigrationId, ProductVersion
        FROM ' + QUOTENAME(@db) + N'.dbo.__EFMigrationsHistory
        WHERE MigrationId = (SELECT MAX(MigrationId) FROM ' + QUOTENAME(@db) + N'.dbo.__EFMigrationsHistory);
    END
    ELSE
    BEGIN
        SELECT N''' + REPLACE(@db,'''','''''') + N''' AS DatabaseName,
               N''__EFMigrationsHistory not found'' AS MigrationId,
               CAST(NULL AS nvarchar(32)) AS ProductVersion;
    END;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM db_cursor INTO @db;
END;

CLOSE db_cursor;
DEALLOCATE db_cursor;
