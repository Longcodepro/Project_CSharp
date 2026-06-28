USE [TuneVaultDb];
GO

IF COL_LENGTH('dbo.Favorites', 'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.Favorites
    ADD IsActive BIT NOT NULL
        CONSTRAINT DF_Favorites_IsActive DEFAULT 1;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Favorites_User_Target'
      AND object_id = OBJECT_ID('dbo.Favorites')
      AND is_unique = 0
)
BEGIN
    IF EXISTS (
        SELECT UserId, TargetType, TargetId
        FROM dbo.Favorites
        GROUP BY UserId, TargetType, TargetId
        HAVING COUNT(1) > 1
    )
    BEGIN
        THROW 50001, N'Khong the chuyen IX_Favorites_User_Target sang unique vi du lieu Favorites dang co dong trung UserId + TargetType + TargetId.', 1;
    END

    DROP INDEX IX_Favorites_User_Target ON dbo.Favorites;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Favorites_User_Target'
      AND object_id = OBJECT_ID('dbo.Favorites')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Favorites_User_Target
    ON dbo.Favorites (UserId, TargetType, TargetId);
END
GO
