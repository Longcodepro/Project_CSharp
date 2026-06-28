USE [TuneVaultDb];
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

BEGIN TRY

IF EXISTS (SELECT 1 FROM dbo.MediaItems WHERE MediaType = 2)
BEGIN
    THROW 50015, N'Khong the gioi han MediaItems.MediaType ve Audio/Video/Song vi van con du lieu MediaType = 2 (Podcast). Hay chuyen hoac xoa du lieu nay truoc.', 1;
END

IF EXISTS (SELECT 1 FROM dbo.Albums WHERE ContentType = 2)
BEGIN
    THROW 50016, N'Khong the gioi han Albums.ContentType ve Audio/Video/Song vi van con du lieu ContentType = 2 (Podcast). Hay chuyen hoac xoa du lieu nay truoc.', 1;
END

IF EXISTS (SELECT 1 FROM dbo.Playlists WHERE ContentType = 2)
BEGIN
    THROW 50017, N'Khong the gioi han Playlists.ContentType ve Audio/Video/Song vi van con du lieu ContentType = 2 (Podcast). Hay chuyen hoac xoa du lieu nay truoc.', 1;
END

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_MediaItems_MediaType'
      AND parent_object_id = OBJECT_ID(N'dbo.MediaItems')
)
BEGIN
    ALTER TABLE dbo.MediaItems DROP CONSTRAINT CK_MediaItems_MediaType;
END

ALTER TABLE dbo.MediaItems
ADD CONSTRAINT CK_MediaItems_MediaType CHECK (MediaType IN (0, 1, 3));

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Albums_ContentType'
      AND parent_object_id = OBJECT_ID(N'dbo.Albums')
)
BEGIN
    ALTER TABLE dbo.Albums DROP CONSTRAINT CK_Albums_ContentType;
END

ALTER TABLE dbo.Albums
ADD CONSTRAINT CK_Albums_ContentType CHECK (ContentType IS NULL OR ContentType IN (0, 1, 3));

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Playlists_ContentType'
      AND parent_object_id = OBJECT_ID(N'dbo.Playlists')
)
BEGIN
    ALTER TABLE dbo.Playlists DROP CONSTRAINT CK_Playlists_ContentType;
END

ALTER TABLE dbo.Playlists
ADD CONSTRAINT CK_Playlists_ContentType CHECK (ContentType IS NULL OR ContentType IN (0, 1, 3));

COMMIT TRANSACTION;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
