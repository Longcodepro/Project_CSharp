USE [TuneVaultDb];
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

BEGIN TRY

DECLARE @LegacyTables TABLE (TableName sysname PRIMARY KEY);
INSERT INTO @LegacyTables (TableName)
VALUES
    (N'MediaArtists'),
    (N'UserAccountTiers'),
    (N'AccountTiers'),
    (N'Admins'),
    (N'Ads');

DECLARE @sql nvarchar(max) = N'';

-- Drop every foreign key that either belongs to or references a legacy table.
SELECT @sql = @sql
    + N'ALTER TABLE '
    + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.'
    + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10)
FROM sys.foreign_keys fk
WHERE OBJECT_NAME(fk.parent_object_id) IN (SELECT TableName FROM @LegacyTables)
   OR OBJECT_NAME(fk.referenced_object_id) IN (SELECT TableName FROM @LegacyTables);

IF @sql <> N''
    EXEC sys.sp_executesql @sql;

IF OBJECT_ID(N'dbo.MediaArtists', N'U') IS NOT NULL DROP TABLE dbo.MediaArtists;
IF OBJECT_ID(N'dbo.UserAccountTiers', N'U') IS NOT NULL DROP TABLE dbo.UserAccountTiers;
IF OBJECT_ID(N'dbo.AccountTiers', N'U') IS NOT NULL DROP TABLE dbo.AccountTiers;
IF OBJECT_ID(N'dbo.Admins', N'U') IS NOT NULL DROP TABLE dbo.Admins;
IF OBJECT_ID(N'dbo.Ads', N'U') IS NOT NULL DROP TABLE dbo.Ads;

DECLARE @LegacyMediaColumns TABLE (ColumnName sysname PRIMARY KEY);
INSERT INTO @LegacyMediaColumns (ColumnName)
VALUES
    (N'AccessLevel'),
    (N'TrailerSeconds'),
    (N'TrailerMinutes'),
    (N'IsValid');

SET @sql = N'';

-- Default and check constraints must be removed before their columns.
SELECT @sql = @sql
    + N'ALTER TABLE dbo.MediaItems DROP CONSTRAINT '
    + QUOTENAME(dc.name) + N';' + CHAR(10)
FROM sys.default_constraints dc
INNER JOIN sys.columns c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.MediaItems')
  AND c.name IN (SELECT ColumnName FROM @LegacyMediaColumns);

SELECT @sql = @sql
    + N'ALTER TABLE dbo.MediaItems DROP CONSTRAINT '
    + QUOTENAME(cc.name) + N';' + CHAR(10)
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID(N'dbo.MediaItems')
  AND (
      cc.name = N'CK_MediaItems_AccessLevel'
      OR cc.definition LIKE N'%AccessLevel%'
      OR cc.definition LIKE N'%TrailerSeconds%'
      OR cc.definition LIKE N'%TrailerMinutes%'
      OR cc.definition LIKE N'%IsValid%'
  );

IF @sql <> N''
    EXEC sys.sp_executesql @sql;

IF COL_LENGTH(N'dbo.MediaItems', N'AccessLevel') IS NOT NULL
    ALTER TABLE dbo.MediaItems DROP COLUMN AccessLevel;
IF COL_LENGTH(N'dbo.MediaItems', N'TrailerSeconds') IS NOT NULL
    ALTER TABLE dbo.MediaItems DROP COLUMN TrailerSeconds;
IF COL_LENGTH(N'dbo.MediaItems', N'TrailerMinutes') IS NOT NULL
    ALTER TABLE dbo.MediaItems DROP COLUMN TrailerMinutes;
IF COL_LENGTH(N'dbo.MediaItems', N'IsValid') IS NOT NULL
    ALTER TABLE dbo.MediaItems DROP COLUMN IsValid;

COMMIT TRANSACTION;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
