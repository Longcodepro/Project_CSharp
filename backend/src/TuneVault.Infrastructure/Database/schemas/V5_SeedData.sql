USE [TuneVault];
GO

/*
    Seed data đồng bộ với cấu trúc hiện tại của TuneVault.
    - File media nằm trong backend/src/TuneVault.API/wwwroot/uploads/media
    - Ảnh bìa nằm trong backend/src/TuneVault.API/wwwroot/uploads/media-covers
    - Canvas nằm trong backend/src/TuneVault.API/wwwroot/uploads/canvas

    Lưu ý:
    - PasswordHash ở đây là placeholder cho dữ liệu dev. Nếu cần đăng nhập thật,
      thay bằng hash bcrypt hợp lệ cho mật khẩu mong muốn.
*/

SET NOCOUNT ON;
GO

INSERT INTO dbo.Users (
    Id,
    IdDisplay,
    DisplayName,
    Email,
    PasswordHash,
    AvatarUrl,
    Bio,
    IsArtist,
    TotalFollowers,
    CreatedAt,
    IsActive
)
VALUES
('U001', 'listener_user', 'Listener User', 'listener@example.com', '$2a$10$placeholderplaceholderplaceholderplaceholderplaceholderpl', '/uploads/avatars/Default.png', NULL, 0, 0, GETUTCDATE(), 1),
('U002', 'sontungmtp', 'Sơn Tùng M-TP', 'sontung@example.com', '$2a$10$placeholderplaceholderplaceholderplaceholderplaceholderpl', '/uploads/avatars/SonTung.png', N'Artist seed account', 1, 1234, GETUTCDATE(), 1),
('U003', 'justinbieber', 'Justin Bieber', 'justin@example.com', '$2a$10$placeholderplaceholderplaceholderplaceholderplaceholderpl', '/uploads/avatars/JustinBieber.png', N'Artist seed account', 1, 998, GETUTCDATE(), 1),
('U004', 'shakira', 'Shakira', 'shakira@example.com', '$2a$10$placeholderplaceholderplaceholderplaceholderplaceholderpl', '/uploads/avatars/Shakira.png', N'Artist seed account', 1, 876, GETUTCDATE(), 1),
('U005', 'mono', 'MONO', 'mono@example.com', '$2a$10$placeholderplaceholderplaceholderplaceholderplaceholderpl', '/uploads/avatars/Mono.png', N'Artist seed account', 1, 542, GETUTCDATE(), 1),
('U006', 'soobin', 'Soobin', 'soobin@example.com', '$2a$10$placeholderplaceholderplaceholderplaceholderplaceholderpl', '/uploads/avatars/Soobin.png', N'Artist seed account', 1, 720, GETUTCDATE(), 1);
GO

INSERT INTO dbo.MediaItems (
    Id,
    OwnerId,
    Title,
    Description,
    MediaType,
    AudioUrl,
    VideoUrl,
    CoverImageUrl,
    CanvasUrl,
    Genre,
    DurationSeconds,
    TrailerSeconds,
    AccessLevel,
    IsPublic,
    FavoriteCount,
    ViewCount,
    UploadedAt,
    ReleaseDate
)
VALUES
('M001', 'U002', N'Hãy Trao Cho Anh', N'Bản nhạc Latin pop sôi động.', 3, '/uploads/media/hay-trao-cho-anh.mp3', NULL, '/uploads/media-covers/hay-trao-cho-anh.jpg', '/uploads/canvas/hay-trao-cho-anh.mp4', N'Latin Pop, Dance', 244, 0, 0, 1, 84, 1200, GETUTCDATE(), NULL),
('M002', 'U002', N'Chạy Ngay Đi', N'Bản nhạc pop căng và giàu năng lượng.', 3, '/uploads/media/chay-ngay-di.mp3', NULL, '/uploads/media-covers/chay-ngay-di.jpg', NULL, N'Pop, EDM', 229, 0, 0, 1, 66, 980, GETUTCDATE(), NULL),
('M003', 'U002', N'Chúng Ta Của Hiện Tại', N'Bản ballad pop nhiều cảm xúc.', 3, '/uploads/media/chung-ta-cua-hien-tai.mp3', NULL, '/uploads/media-covers/chung-ta-cua-hien-tai.png', '/uploads/canvas/chung-ta-cua-hien-tai.mp4', N'Pop Ballad', 272, 0, 0, 1, 91, 1400, GETUTCDATE(), NULL),
('M004', 'U002', N'Come My Way', N'Bản nhạc quốc tế mang màu sắc hiện đại.', 3, '/uploads/media/come-my-way.mp3', NULL, '/uploads/media-covers/come-my-way.png', '/uploads/canvas/come-my-way.mp4', N'Pop, Electronic', 236, 0, 0, 1, 58, 810, GETUTCDATE(), NULL),
('M005', 'U003', N'Baby', N'Bản hit teen pop quen thuộc.', 3, '/uploads/media/baby.mp3', NULL, '/uploads/media-covers/baby.png', NULL, N'Teen Pop, Pop', 214, 0, 0, 1, 102, 1550, GETUTCDATE(), NULL),
('M006', 'U003', N'Beauty And A Beat', N'Bản nhạc dance pop sôi động.', 3, '/uploads/media/beauty-and-a-beat.mp3', NULL, '/uploads/media-covers/beauty-and-a-beat.png', '/uploads/canvas/beauty-and-a-beat.mp4', N'Dance Pop, Electropop', 230, 0, 0, 1, 74, 1240, GETUTCDATE(), NULL),
('M007', 'U004', N'La La La Brasil 2014', N'Bản nhạc giàu năng lượng ngày hội bóng đá.', 3, '/uploads/media/la-la-la-brasil-2014.mp3', NULL, '/uploads/media-covers/la-la-la-brasil-2014.png', NULL, N'Pop, World Cup', 267, 0, 0, 1, 49, 760, GETUTCDATE(), NULL),
('M008', 'U004', N'Hips Don''t Lie', N'Bản Latin pop nổi tiếng toàn cầu.', 3, '/uploads/media/hips-dont-lie.mp3', NULL, '/uploads/media-covers/hips-dont-lie.png', NULL, N'Latin Pop, Dance', 220, 0, 0, 1, 88, 1380, GETUTCDATE(), NULL),
('M009', 'U005', N'Em Xinh', N'Bản nhạc pop trẻ trung.', 3, '/uploads/media/em-xinh.mp3', NULL, '/uploads/media-covers/em-xinh.png', NULL, N'Pop, R&B', 248, 0, 0, 1, 43, 690, GETUTCDATE(), NULL),
('M010', 'U005', N'Waiting For You', N'Bản ballad giàu cảm xúc.', 3, '/uploads/media/waiting-for-you.mp3', NULL, '/uploads/media-covers/waiting-for-you.png', NULL, N'Ballad, Pop', 252, 0, 0, 1, 77, 1110, GETUTCDATE(), NULL),
('M011', 'U006', N'Em', N'Bản nhạc pop buồn nhẹ.', 3, '/uploads/media/em.mp3', NULL, '/uploads/media-covers/em.png', NULL, N'Ballad, Pop', 238, 0, 0, 1, 69, 930, GETUTCDATE(), NULL),
('M012', 'U006', N'Xin Đừng Lặng Im', N'Bản nhạc buồn và sâu lắng.', 3, '/uploads/media/xin-dung-lang-im.mp3', NULL, '/uploads/media-covers/xin-dung-lang-im.png', NULL, N'Ballad, Pop', 246, 0, 0, 1, 81, 1170, GETUTCDATE(), NULL);
GO

INSERT INTO dbo.MediaArtists (
    MediaItemId,
    ArtistId,
    [Role]
)
VALUES
('M001', 'U002', 'MainArtist'),
('M002', 'U002', 'MainArtist'),
('M003', 'U002', 'MainArtist'),
('M004', 'U002', 'MainArtist'),
('M005', 'U003', 'MainArtist'),
('M006', 'U003', 'MainArtist'),
('M007', 'U004', 'MainArtist'),
('M008', 'U004', 'MainArtist'),
('M009', 'U005', 'MainArtist'),
('M010', 'U005', 'MainArtist'),
('M011', 'U006', 'MainArtist'),
('M012', 'U006', 'MainArtist');
GO
