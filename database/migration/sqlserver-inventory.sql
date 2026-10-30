/*
 VamCity SQL Server metadata inventory (READ ONLY)
 Run in the source database after selecting the confirmed database:
   USE [vamcitydigibilepay];
 This script reads catalog metadata only. It does not export customer rows or modify schema/data.
 Each SELECT returns one inventory result set.
*/
SET NOCOUNT ON;

-- Database identity and collation
SELECT DB_NAME() AS database_name, d.collation_name, d.compatibility_level
FROM sys.databases AS d WHERE d.database_id = DB_ID();

-- Tables and row counts (row counts are catalog estimates until validated)
SELECT s.name AS schema_name, t.name AS table_name, SUM(p.rows) AS catalog_row_count
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id=t.schema_id
LEFT JOIN sys.partitions AS p ON p.object_id=t.object_id AND p.index_id IN (0,1)
GROUP BY s.name,t.name ORDER BY s.name,t.name;

-- Columns, SQL types, nullability, identity, defaults and computed expressions
SELECT s.name AS schema_name,t.name AS table_name,c.column_id,c.name AS column_name,
       ty.name AS sql_type,c.max_length,c.precision,c.scale,c.is_nullable,
       c.is_identity,c.is_computed,dc.definition AS default_definition,
       cc.definition AS computed_definition
FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
JOIN sys.columns c ON c.object_id=t.object_id
JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
LEFT JOIN sys.computed_columns cc ON cc.object_id=c.object_id AND cc.column_id=c.column_id
ORDER BY s.name,t.name,c.column_id;

-- Primary keys and unique constraints
SELECT s.name AS schema_name,t.name AS table_name,
       i.name AS constraint_or_index_name,i.is_primary_key,i.is_unique,
       ic.key_ordinal,c.name AS column_name,ic.is_descending_key
FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
JOIN sys.indexes i ON i.object_id=t.object_id AND (i.is_primary_key=1 OR i.is_unique_constraint=1 OR i.is_unique=1)
JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id AND ic.key_ordinal>0
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
ORDER BY s.name,t.name,i.name,ic.key_ordinal;

-- Foreign keys and column mappings
SELECT ps.name AS parent_schema,pt.name AS parent_table,pc.name AS parent_column,
       rs.name AS referenced_schema,rt.name AS referenced_table,rc.name AS referenced_column,
       fk.name AS foreign_key_name,fkc.constraint_column_id,fk.delete_referential_action_desc,
       fk.update_referential_action_desc,fk.is_disabled,fk.is_not_trusted
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
JOIN sys.tables pt ON pt.object_id=fkc.parent_object_id
JOIN sys.schemas ps ON ps.schema_id=pt.schema_id
JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id
JOIN sys.schemas rs ON rs.schema_id=rt.schema_id
JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
ORDER BY ps.name,pt.name,fk.name,fkc.constraint_column_id;

-- Indexes and indexed columns
SELECT s.name AS schema_name,t.name AS table_name,i.name AS index_name,
       i.type_desc,i.is_unique,i.is_primary_key,i.is_disabled,
       ic.key_ordinal,ic.is_included_column,c.name AS column_name
FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id
JOIN sys.schemas s ON s.schema_id=t.schema_id
JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE i.index_id>0
ORDER BY s.name,t.name,i.name,ic.key_ordinal,ic.index_column_id;

-- Views, procedures, functions and triggers
SELECT 'VIEW' AS object_type,s.name AS schema_name,o.name AS object_name,m.definition
FROM sys.views o JOIN sys.schemas s ON s.schema_id=o.schema_id
LEFT JOIN sys.sql_modules m ON m.object_id=o.object_id
UNION ALL
SELECT CASE WHEN o.type='P' THEN 'PROCEDURE' ELSE 'FUNCTION' END,s.name,o.name,m.definition
FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
LEFT JOIN sys.sql_modules m ON m.object_id=o.object_id
WHERE o.type IN ('P','FN','IF','TF','FS','FT')
UNION ALL
SELECT 'TRIGGER',OBJECT_SCHEMA_NAME(tr.parent_id),tr.name,m.definition
FROM sys.triggers tr LEFT JOIN sys.sql_modules m ON m.object_id=tr.object_id
ORDER BY object_type,schema_name,object_name;

-- Check constraints
SELECT s.name AS schema_name,t.name AS table_name,cc.name AS constraint_name,
       cc.definition,cc.is_disabled,cc.is_not_trusted
FROM sys.check_constraints cc JOIN sys.tables t ON t.object_id=cc.parent_object_id
JOIN sys.schemas s ON s.schema_id=t.schema_id
ORDER BY s.name,t.name,cc.name;

-- Sequences and user-defined types
SELECT SCHEMA_NAME(schema_id) AS schema_name,name AS sequence_name,
       TYPE_NAME(user_type_id) AS data_type,start_value,increment,minimum_value,maximum_value,is_cycling
FROM sys.sequences ORDER BY schema_name,sequence_name;

SELECT SCHEMA_NAME(schema_id) AS schema_name,name AS type_name,
       TYPE_NAME(system_type_id) AS base_type,max_length,precision,scale,is_nullable
FROM sys.types WHERE is_user_defined=1 ORDER BY schema_name,type_name;
