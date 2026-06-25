USE TuneVault;
GO

IF COL_LENGTH('dbo.Favorites', 'TargetId') IS NULL
BEGIN
    ALTER TABLE dbo.Favorites
    ADD TargetId varchar(10) NULL;
END
GO

IF COL_LENGTH('dbo.Favorites', 'TargetType') IS NULL
BEGIN
    ALTER TABLE dbo.Favorites
    ADD TargetType tinyint NOT NULL
        CONSTRAINT DF_Favorites_TargetType DEFAULT 0;
END
GO

UPDATE dbo.Favorites
SET TargetId = MediaItemId
WHERE TargetId IS NULL;
GO

ALTER TABLE dbo.Favorites
ALTER COLUMN TargetId varchar(10) NOT NULL;
GO

ALTER TABLE dbo.Favorites
ALTER COLUMN MediaItemId varchar(10) NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Favorites_TargetType'
      AND parent_object_id = OBJECT_ID('dbo.Favorites')
)
BEGIN
    ALTER TABLE dbo.Favorites
    ADD CONSTRAINT CK_Favorites_TargetType CHECK (TargetType IN (0, 1, 2));
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Favorites_User_Target'
      AND object_id = OBJECT_ID('dbo.Favorites')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Favorites_User_Target
    ON dbo.Favorites (UserId, TargetType, TargetId);
END
GO
