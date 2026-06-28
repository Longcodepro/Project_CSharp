/*
    DEV ONLY - Clear toàn bộ dữ liệu TuneVault.

    Script này chỉ xóa dữ liệu, không xóa bảng/cột/constraint.
    Viết theo T-SQL thuần để chạy được trong DBeaver trên LinuxMint.
*/

USE [TuneVaultDb];

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @sql NVARCHAR(MAX) = N'';

    -- Tắt toàn bộ constraint trên user tables để xóa được theo bất kỳ thứ tự nào.
    SELECT @sql = STRING_AGG(
        N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N' NOCHECK CONSTRAINT ALL;',
        N''
    )
    FROM sys.tables AS t;

    SET @sql = COALESCE(@sql, N'');
    IF (@sql <> N'')
        EXEC sys.sp_executesql @sql;

    SET @sql = N'';

    -- Xóa dữ liệu của mọi user table.
    SELECT @sql = STRING_AGG(
        N'DELETE FROM ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N';',
        N''
    ) WITHIN GROUP (ORDER BY t.object_id DESC)
    FROM sys.tables AS t;

    SET @sql = COALESCE(@sql, N'');
    IF (@sql <> N'')
        EXEC sys.sp_executesql @sql;

    SET @sql = N'';

    -- Nếu có identity column thì reseed về 0.
    SELECT @sql = STRING_AGG(
        N'DBCC CHECKIDENT ('''
        + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
        + N''', RESEED, 0);',
        N''
    ) WITHIN GROUP (ORDER BY t.object_id)
    FROM sys.identity_columns AS ic
    INNER JOIN sys.tables AS t ON t.object_id = ic.object_id;

    IF (@sql <> N'')
    BEGIN
        EXEC sys.sp_executesql @sql;
    END

    SET @sql = N'';

    -- Bật lại constraint sau khi dữ liệu đã sạch.
    SELECT @sql = STRING_AGG(
        N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N' WITH CHECK CHECK CONSTRAINT ALL;',
        N''
    )
    FROM sys.tables AS t;

    SET @sql = COALESCE(@sql, N'');
    IF (@sql <> N'')
        EXEC sys.sp_executesql @sql;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
