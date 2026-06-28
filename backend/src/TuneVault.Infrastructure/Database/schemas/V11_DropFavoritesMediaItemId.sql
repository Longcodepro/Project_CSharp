USE [TuneVaultDb];

IF COL_LENGTH('dbo.Favorites', 'MediaItemId') IS NOT NULL
BEGIN
    DECLARE @favoriteMediaFk sysname;

    SELECT TOP (1)
        @favoriteMediaFk = fk.name
    FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
        ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c
        ON c.object_id = fkc.parent_object_id
            AND c.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Favorites')
        AND c.name = N'MediaItemId';

    IF @favoriteMediaFk IS NOT NULL
    BEGIN
        DECLARE @dropSql nvarchar(max);
        SET @dropSql = N'ALTER TABLE dbo.Favorites DROP CONSTRAINT ' + QUOTENAME(@favoriteMediaFk) + N';';
        EXEC sp_executesql @dropSql;
    END

    ALTER TABLE dbo.Favorites
    DROP COLUMN MediaItemId;
END
