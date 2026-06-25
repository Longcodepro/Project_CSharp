/*
    DEV ONLY - Clear toàn bộ dữ liệu TuneVault.

    Script này chỉ xóa dữ liệu, không xóa bảng/cột/constraint.
    Chạy khi bạn muốn làm sạch database local để seed dữ liệu chuẩn lại.
*/

USE [TuneVault];
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Các bảng quan hệ phụ thuộc Media/Playlist/Album/User.
    IF OBJECT_ID(N'dbo.PlaylistTracks', N'U') IS NOT NULL DELETE FROM dbo.PlaylistTracks;
    IF OBJECT_ID(N'dbo.AlbumTracks', N'U') IS NOT NULL DELETE FROM dbo.AlbumTracks;
    IF OBJECT_ID(N'dbo.MediaArtists', N'U') IS NOT NULL DELETE FROM dbo.MediaArtists;
    IF OBJECT_ID(N'dbo.Favorites', N'U') IS NOT NULL DELETE FROM dbo.Favorites;
    IF OBJECT_ID(N'dbo.PlayHistory', N'U') IS NOT NULL DELETE FROM dbo.PlayHistory;
    IF OBJECT_ID(N'dbo.MediaShares', N'U') IS NOT NULL DELETE FROM dbo.MediaShares;
    IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL DELETE FROM dbo.Notifications;

    -- Quan hệ người dùng.
    IF OBJECT_ID(N'dbo.Follows', N'U') IS NOT NULL DELETE FROM dbo.Follows;
    IF OBJECT_ID(N'dbo.Friends', N'U') IS NOT NULL DELETE FROM dbo.Friends;
    IF OBJECT_ID(N'dbo.UserAccountTiers', N'U') IS NOT NULL DELETE FROM dbo.UserAccountTiers;
    IF OBJECT_ID(N'dbo.OtpLogs', N'U') IS NOT NULL DELETE FROM dbo.OtpLogs;

    -- Bảng nội dung chính.
    IF OBJECT_ID(N'dbo.Playlists', N'U') IS NOT NULL DELETE FROM dbo.Playlists;
    IF OBJECT_ID(N'dbo.Albums', N'U') IS NOT NULL DELETE FROM dbo.Albums;
    IF OBJECT_ID(N'dbo.MediaItems', N'U') IS NOT NULL DELETE FROM dbo.MediaItems;

    -- Bảng gốc và lookup. Xóa luôn để database thật sự rỗng.
    IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DELETE FROM dbo.Users;
    IF OBJECT_ID(N'dbo.Admins', N'U') IS NOT NULL DELETE FROM dbo.Admins;
    IF OBJECT_ID(N'dbo.Ads', N'U') IS NOT NULL DELETE FROM dbo.Ads;
    IF OBJECT_ID(N'dbo.AccountTiers', N'U') IS NOT NULL DELETE FROM dbo.AccountTiers;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
