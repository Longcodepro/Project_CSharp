USE TuneVault;

INSERT INTO Users
(
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
SELECT *
FROM
(
    VALUES
    (
        'U003',
        'sontungmtp',
        N'Sơn Tùng M-TP',
        'sontungmtp@gmail.com',
        '$2y$10$b8V6c1j.hx0YvqDUTk3MBOwORLdarDduKojOyjs2UATedO91t4DYG',
        '/uploads/avatars/SonTung.png',
        N'Sơn Tùng M-TP, tên thật Nguyễn Thanh Tùng, sinh năm 1994 tại Thái Bình. Anh là ca sĩ, nhạc sĩ và diễn viên Việt Nam, nổi bật với phong cách pop, R&B, EDM. Một số ca khúc tiêu biểu gồm Em của ngày hôm qua, Chạy ngay đi, Hãy trao cho anh và Chúng ta của hiện tại.',
        1,
        0,
        SYSDATETIME(),
        1
    ),
    (
        'U004',
        'justinbieber',
        N'Justin Bieber',
        'justinbieber@gmail.com',
        '$2y$10$lraDeAp5v26hrXi59xdUjOnETzr04XdsOURQzsBcdmfN/JO0Hnq6G',
        '/uploads/avatars/JustinBieber.png',
        N'Justin Bieber, tên đầy đủ Justin Drew Bieber, sinh năm 1994 tại Canada. Anh là ca sĩ nhạc pop nổi tiếng toàn cầu với nhiều bản hit như Baby, Sorry, Love Yourself, What Do You Mean và Peaches.',
        1,
        0,
        SYSDATETIME(),
        1
    ),
    (
        'U005',
        'shakira',
        N'Shakira',
        'shakira@gmail.com',
        '$2y$10$.EMQ87CsR.G79piu0ZbTturT/ySOoUudvW9BG/XVfzPjYr/8IogPy',
        '/uploads/avatars/Shakira.png',
        N'Shakira, tên đầy đủ Shakira Isabel Mebarak Ripoll, sinh năm 1977 tại Colombia. Cô là ca sĩ, nhạc sĩ và vũ công nổi tiếng toàn cầu, gắn liền với Latin pop. Một số ca khúc tiêu biểu gồm Hips Dont Lie, Waka Waka và Whenever Wherever.',
        1,
        0,
        SYSDATETIME(),
        1
    ),
    (
        'U006',
        'mono',
        N'MONO',
        'mono@gmail.com',
        '$2y$10$lphI.M0IecPIRBj2KX.5c.mK56gyDtflGQaeeFUpWnYNzn3nEaB.e',
        '/uploads/avatars/Mono.png',
        N'MONO, tên thật Nguyễn Việt Hoàng, sinh năm 2000 tại Thái Bình. Anh là ca sĩ trẻ của V-pop, được biết đến với phong cách âm nhạc hiện đại, trẻ trung và năng động. Một số ca khúc tiêu biểu gồm Waiting For You, Em Là và Quên Anh Đi.',
        1,
        0,
        SYSDATETIME(),
        1
    ),
    (
        'U007',
        'soobin',
        N'SOOBIN',
        'soobin@gmail.com',
        '$2y$10$0kZQfuU7iaB72fbwBArDxOgeyoB26sITw3ZzASfNWvsU48wBQj/a6',
        '/uploads/avatars/Soobin.png',
        N'SOOBIN, tên thật Nguyễn Huỳnh Sơn, sinh năm 1992 tại Hà Nội. Anh là ca sĩ, nhạc sĩ Việt Nam, nổi bật với pop, R&B và ballad. Một số ca khúc tiêu biểu gồm Phía Sau Một Cô Gái, Xin Đừng Lặng Im, Nếu Ngày Ấy và Tháng Năm.',
        1,
        0,
        SYSDATETIME(),
        1
    )
) AS SeedUsers
(
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
WHERE NOT EXISTS
(
    SELECT 1
    FROM Users u
    WHERE u.Id = SeedUsers.Id
       OR u.IdDisplay = SeedUsers.IdDisplay
       OR u.Email = SeedUsers.Email
);



USE TuneVault;

DECLARE @SongMediaType TINYINT = 3;      -- MediaType.Song
DECLARE @NormalAccessLevel TINYINT = 0;  -- AccessLevel.Normal

MERGE MediaItems AS Target
USING
(
    VALUES
    -- Sơn Tùng M-TP - UserId hiện tại: U003
    (
        'M001',
        'U003',
        N'Hãy Trao Cho Anh',
        N'Ca khúc pop/R&B mang màu sắc Latin hiện đại, giai điệu bắt tai và không khí sôi động. Phù hợp để demo playlist mùa hè, tiệc tùng hoặc nhạc nổi bật.',
        @SongMediaType,
        '/uploads/media/sontungmtp/hay-trao-cho-anh.mp3',
        NULL,
        '/uploads/covers/sontungmtp/hay-trao-cho-anh.jpg',
        '/uploads/canvas/hay-trao-cho-anh.mp4',
        N'Pop, R&B, Latin Pop',
        245,
        30,
        @NormalAccessLevel,
        1,
        120,
        2500,
        CAST('2019-07-01' AS DATETIME2),
        '/uploads/media/sontungmtp/hay-trao-cho-anh.mp3',
        4,
        0,
        0
    ),
    (
        'M002',
        'U003',
        N'Chạy Ngay Đi',
        N'Bài hát dark pop/R&B với nhịp điệu mạnh, không khí bí ẩn và cảm xúc dứt khoát. Phù hợp để demo nhóm bài hát trending hoặc phong cách cá tính.',
        @SongMediaType,
        '/uploads/media/sontungmtp/chay-ngay-di.mp3',
        NULL,
        '/uploads/covers/sontungmtp/chay-ngay-di.jpg',
        NULL,
        N'Dark Pop, R&B',
        248,
        30,
        @NormalAccessLevel,
        1,
        98,
        2100,
        CAST('2018-05-12' AS DATETIME2),
        '/uploads/media/sontungmtp/chay-ngay-di.mp3',
        4,
        0,
        0
    ),
    (
        'M003',
        'U003',
        N'Chúng Ta Của Hiện Tại',
        N'Bản pop ballad nhẹ nhàng, giàu cảm xúc, kể về tình yêu, ký ức và những khoảnh khắc đẹp trong hiện tại. Phù hợp để demo playlist thư giãn.',
        @SongMediaType,
        '/uploads/media/sontungmtp/chung-ta-cua-hien-tai.mp3',
        NULL,
        '/uploads/covers/sontungmtp/chung-ta-cua-hien-tai.jpg',
        '/uploads/canvas/chung-ta-cua-hien-tai.mp4',
        N'Pop Ballad',
        302,
        30,
        @NormalAccessLevel,
        1,
        135,
        3100,
        CAST('2020-12-20' AS DATETIME2),
        '/uploads/media/sontungmtp/chung-ta-cua-hien-tai.mp3',
        5,
        0,
        0
    ),
    (
        'M004',
        'U003',
        N'Come My Way',
        N'Ca khúc hiện đại, sôi động và có hơi hướng quốc tế. Giai điệu bắt tai, phù hợp để demo playlist party, workout hoặc bài hát đề xuất cho người nghe trẻ.',
        @SongMediaType,
        '/uploads/media/sontungmtp/come-my-way.mp3',
        NULL,
        '/uploads/covers/sontungmtp/come-my-way.jpg',
        NULL,
        N'Pop, Hip-hop',
        230,
        30,
        @NormalAccessLevel,
        1,
        76,
        1700,
        NULL,
        '/uploads/media/sontungmtp/come-my-way.mp3',
        3,
        0,
        0
    ),

    -- Justin Bieber - UserId hiện tại: U004
    (
        'M005',
        'U004',
        N'Baby',
        N'Bài hát teen pop vui tươi, dễ nhớ, nói về cảm xúc rung động và tình yêu tuổi trẻ. Phù hợp để demo nhạc pop quốc tế hoặc bài hát phổ biến.',
        @SongMediaType,
        '/uploads/media/justinbieber/baby.mp3',
        NULL,
        '/uploads/covers/justinbieber/baby.jpg',
        NULL,
        N'Teen Pop, Pop',
        214,
        30,
        @NormalAccessLevel,
        1,
        160,
        4200,
        CAST('2010-01-18' AS DATETIME2),
        '/uploads/media/justinbieber/baby.mp3',
        3,
        0,
        0
    ),
    (
        'M006',
        'U004',
        N'Beauty And A Beat',
        N'Ca khúc dance-pop/electropop có tiết tấu mạnh, không khí tiệc tùng và năng lượng cao. Phù hợp để demo playlist party, dance hoặc workout.',
        @SongMediaType,
        '/uploads/media/justinbieber/beauty-and-a-beat.mp3',
        NULL,
        '/uploads/covers/justinbieber/beauty-and-a-beat.jpg',
        '/uploads/canvas/beauty-and-a-beat.mp4',
        N'Dance Pop, Electropop',
        228,
        30,
        @NormalAccessLevel,
        1,
        115,
        2900,
        CAST('2012-10-24' AS DATETIME2),
        '/uploads/media/justinbieber/beauty-and-a-beat.mp3',
        3,
        0,
        0
    ),

    -- Shakira - UserId hiện tại: U005
    (
        'M007',
        'U005',
        N'La La La (Brasil 2014)',
        N'Ca khúc dance-pop Latin có tiết tấu nhanh, vui tươi và không khí lễ hội. Phù hợp để demo playlist thể thao, nhảy, mùa hè hoặc nhạc sôi động.',
        @SongMediaType,
        '/uploads/media/shakira/la-la-la-brasil-2014.mp3',
        NULL,
        '/uploads/covers/shakira/la-la-la-brasil-2014.jpg',
        NULL,
        N'Dance Pop, Latin Pop',
        197,
        30,
        @NormalAccessLevel,
        1,
        145,
        3600,
        CAST('2014-05-27' AS DATETIME2),
        '/uploads/media/shakira/la-la-la-brasil-2014.mp3',
        3,
        0,
        0
    ),
    (
        'M008',
        'U005',
        N'Hips Don''t Lie',
        N'Bài hát Latin pop/reggaeton nổi bật với tiết tấu sôi động, giai điệu cuốn hút và phong cách trình diễn đặc trưng. Phù hợp cho playlist dance.',
        @SongMediaType,
        '/uploads/media/shakira/hips-dont-lie.mp3',
        NULL,
        '/uploads/covers/shakira/hips-dont-lie.jpg',
        NULL,
        N'Latin Pop, Reggaeton',
        218,
        30,
        @NormalAccessLevel,
        1,
        180,
        5000,
        CAST('2006-02-28' AS DATETIME2),
        '/uploads/media/shakira/hips-dont-lie.mp3',
        3,
        0,
        0
    ),

    -- MONO - UserId hiện tại: U006
    (
        'M009',
        'U006',
        N'Em Xinh',
        N'Ca khúc V-pop trẻ trung, giai điệu bắt tai, mang màu sắc hiện đại và vui vẻ. Phù hợp để demo nhạc Việt trending hoặc nghe hằng ngày.',
        @SongMediaType,
        '/uploads/media/mono/em-xinh.mp3',
        NULL,
        '/uploads/covers/mono/em-xinh.jpg',
        NULL,
        N'V-pop, Pop',
        210,
        30,
        @NormalAccessLevel,
        1,
        90,
        2300,
        NULL,
        '/uploads/media/mono/em-xinh.mp3',
        3,
        0,
        0
    ),
    (
        'M010',
        'U006',
        N'Waiting For You',
        N'Bài hát pop/R&B hiện đại với giai điệu cuốn hút, không khí lãng mạn và phần hook dễ nhớ. Phù hợp để demo playlist V-pop, chill hoặc romance.',
        @SongMediaType,
        '/uploads/media/mono/waiting-for-you.mp3',
        NULL,
        '/uploads/covers/mono/waiting-for-you.jpg',
        NULL,
        N'V-pop, Pop, R&B',
        265,
        30,
        @NormalAccessLevel,
        1,
        170,
        4600,
        CAST('2022-08-18' AS DATETIME2),
        '/uploads/media/mono/waiting-for-you.mp3',
        4,
        0,
        0
    ),

    -- SOOBIN - UserId hiện tại: U007
    (
        'M011',
        'U007',
        N'Em',
        N'Ca khúc pop/R&B nhẹ nhàng, tập trung vào cảm xúc tình yêu và sự dịu dàng trong cách thể hiện. Phù hợp để demo playlist chill hoặc romance.',
        @SongMediaType,
        '/uploads/media/soobin/em.mp3',
        NULL,
        '/uploads/covers/soobin/em.jpg',
        NULL,
        N'Pop, R&B',
        220,
        30,
        @NormalAccessLevel,
        1,
        70,
        1500,
        NULL,
        '/uploads/media/soobin/em.mp3',
        3,
        0,
        0
    ),
    (
        'M012',
        'U007',
        N'Xin Đừng Lặng Im',
        N'Bản ballad buồn, giàu cảm xúc, nói về sự im lặng và khoảng cách trong tình yêu. Phù hợp để demo playlist nhạc buồn hoặc nghe đêm khuya.',
        @SongMediaType,
        '/uploads/media/soobin/xin-dung-lang-im.mp3',
        NULL,
        '/uploads/covers/soobin/xin-dung-lang-im.jpg',
        NULL,
        N'Ballad, V-pop',
        260,
        30,
        @NormalAccessLevel,
        1,
        105,
        2700,
        CAST('2017-07-18' AS DATETIME2),
        '/uploads/media/soobin/xin-dung-lang-im.mp3',
        4,
        0,
        0
    )
) AS Source
(
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
    ReleaseDate,
    Url,
    DurationMinutes,
    TrailerMinutes,
    IsValid
)
ON Target.Id = Source.Id
WHEN MATCHED THEN
    UPDATE SET
        Target.OwnerId = Source.OwnerId,
        Target.Title = Source.Title,
        Target.Description = Source.Description,
        Target.MediaType = Source.MediaType,
        Target.AudioUrl = Source.AudioUrl,
        Target.VideoUrl = Source.VideoUrl,
        Target.CoverImageUrl = Source.CoverImageUrl,
        Target.CanvasUrl = Source.CanvasUrl,
        Target.Genre = Source.Genre,
        Target.DurationSeconds = Source.DurationSeconds,
        Target.TrailerSeconds = Source.TrailerSeconds,
        Target.AccessLevel = Source.AccessLevel,
        Target.IsPublic = Source.IsPublic,
        Target.FavoriteCount = Source.FavoriteCount,
        Target.ViewCount = Source.ViewCount,
        Target.ReleaseDate = Source.ReleaseDate,
        Target.IsActive = 1,
        Target.Url = Source.Url,
        Target.DurationMinutes = Source.DurationMinutes,
        Target.TrailerMinutes = Source.TrailerMinutes,
        Target.IsValid = Source.IsValid
WHEN NOT MATCHED THEN
    INSERT
    (
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
        ReleaseDate,
        IsActive,
        Url,
        DurationMinutes,
        TrailerMinutes,
        IsValid
    )
    VALUES
    (
        Source.Id,
        Source.OwnerId,
        Source.Title,
        Source.Description,
        Source.MediaType,
        Source.AudioUrl,
        Source.VideoUrl,
        Source.CoverImageUrl,
        Source.CanvasUrl,
        Source.Genre,
        Source.DurationSeconds,
        Source.TrailerSeconds,
        Source.AccessLevel,
        Source.IsPublic,
        Source.FavoriteCount,
        Source.ViewCount,
        SYSDATETIME(),
        Source.ReleaseDate,
        1,
        Source.Url,
        Source.DurationMinutes,
        Source.TrailerMinutes,
        Source.IsValid
    );


MERGE MediaArtists AS Target
USING
(
    VALUES
    -- Sơn Tùng M-TP
    ('M001', 'U003', N'MainArtist'),
    ('M002', 'U003', N'MainArtist'),
    ('M003', 'U003', N'MainArtist'),
    ('M004', 'U003', N'MainArtist'),

    -- Justin Bieber
    ('M005', 'U004', N'MainArtist'),
    ('M006', 'U004', N'MainArtist'),

    -- Shakira
    ('M007', 'U005', N'MainArtist'),
    ('M008', 'U005', N'MainArtist'),

    -- MONO
    ('M009', 'U006', N'MainArtist'),
    ('M010', 'U006', N'MainArtist'),

    -- SOOBIN
    ('M011', 'U007', N'MainArtist'),
    ('M012', 'U007', N'MainArtist')
) AS Source
(
    MediaItemId,
    ArtistId,
    Role
)
ON Target.MediaItemId = Source.MediaItemId
AND Target.ArtistId = Source.ArtistId
WHEN MATCHED THEN
    UPDATE SET
        Target.Role = Source.Role
WHEN NOT MATCHED THEN
    INSERT
    (
        MediaItemId,
        ArtistId,
        Role
    )
    VALUES
    (
        Source.MediaItemId,
        Source.ArtistId,
        Source.Role
    );

