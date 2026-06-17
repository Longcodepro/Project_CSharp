// Infrastructure/Repositories/MediaRepository.cs
using Dapper;
using System.IO;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.ValueObject;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// MediaRepository - Lớp quản lý dữ liệu MediaItem trên database.
/// 
/// Chức năng chính:
/// - Thao tác CRUD với MediaItem entities
/// - Quản lý MediaArtist relationships (liên kết giữa Media và Artists)
/// - Tìm kiếm Media theo keyword (Title, Genre)
/// - Deactivate (soft delete) Media items
/// 
/// Sử dụng: Dapper ORM với raw SQL queries để tối ưu performance.
/// </summary>
public sealed class MediaRepository : IMediaRepository
{
    private const string MediaItemSelectColumns = """
        Id,
        OwnerId,
        Title,
        Description,
        MediaType AS [Type],
        CoverImageUrl,
        CanvasUrl,
        Genre,
        AccessLevel,
        IsPublic,
        IsActive,
        IsValid,
        FavoriteCount,
        ViewCount,
        UploadedAt,
        ReleaseDate
        """;

    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo MediaRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database</param>
    public MediaRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    // =========================================================================
    // QUERIES - Các method lấy dữ liệu từ database
    // =========================================================================

    /// <summary>
    /// Lấy MediaItem theo Id, chỉ lấy media đang active (IsActive = 1).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. SELECT * FROM MediaItems WHERE Id = @Id AND IsActive = 1
    /// 3. Trả về MediaItem object hoặc null nếu không tìm thấy
    /// </summary>
    /// <param name="id">Id của MediaItem</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>MediaItem object hoặc null</returns>
    public async Task<MediaItem?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT {MediaItemSelectColumns}
            FROM [MediaItems]
            WHERE Id = @Id AND IsActive = 1
            """;
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<MediaItem>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy danh sách Artists liên kết với một MediaItem.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. SELECT * FROM MediaArtists WHERE MediaItemId = @MediaItemId
    /// 3. Lấy tất cả artists liên kết (có thể là singer, composer, producer, v.v.)
    /// 4. Trả về danh sách MediaArtist objects
    /// </summary>
    /// <param name="mediaItemId">Id của MediaItem</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>IEnumerable&lt;MediaArtist&gt; danh sách artists</returns>
    public async Task<IEnumerable<MediaArtist>> GetArtistsByMediaIdAsync(string mediaItemId, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM [MediaArtists] WHERE MediaItemId = @MediaItemId";
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<MediaArtist>(
            new CommandDefinition(sql, new { MediaItemId = mediaItemId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyCollection<MediaItem>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        const string sql = $"""
            SELECT {MediaItemSelectColumns}
            FROM [MediaItems]
            WHERE IsActive = 1
              AND IsValid = 0
            ORDER BY UploadedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var conn = _db.CreateConnection();
        var items = await conn.QueryAsync<MediaItem>(
            new CommandDefinition(sql, new
            {
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: ct));

        return items.ToList();
    }

    public async Task<IReadOnlyCollection<MediaItem>> GetByOwnerAsync(string ownerId, CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT {MediaItemSelectColumns}
            FROM [MediaItems]
            WHERE IsActive = 1
              AND OwnerId = @OwnerId
            ORDER BY UploadedAt DESC
            """;

        using var conn = _db.CreateConnection();
        var items = await conn.QueryAsync<MediaItem>(
            new CommandDefinition(sql, new { OwnerId = ownerId }, cancellationToken: ct));

        return items.ToList();
    }

    public async Task<MediaStreamInfo?> GetStreamAsync(
        string mediaId,
        MediaAssetKind assetKind = MediaAssetKind.Primary,
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Id, MediaType, AudioUrl, VideoUrl, CoverImageUrl
            FROM [MediaItems]
            WHERE Id = @MediaId AND IsActive = 1 AND IsValid = 0";

        using var conn = _db.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<MediaAssetRow>(
            new CommandDefinition(sql, new { MediaId = mediaId }, cancellationToken: ct));

        if (row is null)
            return null;

        var mediaType = (MediaType)row.MediaType;
        var filePath = assetKind switch
        {
            MediaAssetKind.Audio => row.AudioUrl,
            MediaAssetKind.Video => row.VideoUrl,
            MediaAssetKind.Poster => row.CoverImageUrl,
            _ => mediaType == MediaType.Video ? row.VideoUrl : row.AudioUrl
        };

        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        var contentType = GetContentType(filePath, assetKind == MediaAssetKind.Video ? MediaType.Video : mediaType);
        var supportsRange = true;

        return new MediaStreamInfo(row.Id, filePath, contentType, supportsRange);
    }

    /// <summary>
    /// Tìm kiếm MediaItem theo keyword (Title hoặc Genre).
    /// Chỉ lấy media đang active (IsActive = 1) và public (IsPublic = 1), sắp xếp theo upload mới nhất.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. SELECT * FROM MediaItems WHERE:
    ///    - IsActive = 1 (chỉ lấy media không bị xóa)
    ///    - IsPublic = 1 (chỉ lấy media công khai)
    ///    - Title LIKE @Keyword OR Genre LIKE @Keyword (tìm kiếm linh hoạt)
    /// 3. ORDER BY UploadedAt DESC (media mới nhất trước)
    /// 4. Trả về danh sách MediaItem
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm (ví dụ: "rock", "music")</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>IEnumerable&lt;MediaItem&gt; danh sách kết quả tìm kiếm</returns>
    public async Task<IEnumerable<MediaItem>> SearchAsync(string keyword, CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT {MediaItemSelectColumns}
            FROM [MediaItems]
            WHERE IsActive = 1 AND IsPublic = 1 AND IsValid = 0
              AND (Title LIKE @Keyword OR Genre LIKE @Keyword)
            ORDER BY UploadedAt DESC
            """;

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<MediaItem>(
            new CommandDefinition(sql, new { Keyword = $"%{keyword}%" }, cancellationToken: ct));
    }

    // =========================================================================
    // COMMANDS - Các method thay đổi dữ liệu trong database
    // =========================================================================

    /// <summary>
    /// Thêm MediaItem mới vào database.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL INSERT với các cột: Id, OwnerId, Title, Description, MediaType, 
    ///    AudioUrl, VideoUrl, CoverImageUrl, CanvasUrl, Genre, DurationSeconds, TrailerSeconds,
    ///    AccessLevel, IsPublic, IsActive, FavoriteCount, ViewCount, UploadedAt, ReleaseDate
    /// 3. Trích xuất dữ liệu từ MediaItem entity sang parameters:
    ///    - MediaType: chuyển enum thành int
    ///    - AudioUrl: lấy từ Url.Value nếu không phải video
    ///    - VideoUrl: lấy từ Url.Value nếu là video
    ///    - Duration: chuyển timespan sang seconds
    ///    - AccessLevel: chuyển enum thành int
    /// 4. Thực hiện INSERT
    /// </summary>
    /// <param name="mediaItem">MediaItem entity chứa dữ liệu cần lưu</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    public async Task AddAsync(MediaItem mediaItem, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO [MediaItems]
                (Id, OwnerId, Title, Description, MediaType, AudioUrl, VideoUrl, Url,
                 CoverImageUrl, CanvasUrl, Genre, DurationSeconds, TrailerSeconds,
                 DurationMinutes, TrailerMinutes, AccessLevel, IsPublic, IsActive, IsValid,
                 FavoriteCount, ViewCount, UploadedAt, ReleaseDate)
            VALUES
                (@Id, @OwnerId, @Title, @Description, @MediaType, @AudioUrl, @VideoUrl, @Url,
                 @CoverImageUrl, @CanvasUrl, @Genre, @DurationSeconds, @TrailerSeconds,
                 @DurationMinutes, @TrailerMinutes, @AccessLevel, @IsPublic, @IsActive, @IsValid,
                 @FavoriteCount, @ViewCount, @UploadedAt, @ReleaseDate)";

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            mediaItem.Id,
            mediaItem.OwnerId,
            mediaItem.Title,
            mediaItem.Description,
            MediaType       = (int)mediaItem.Type,
            AudioUrl        = mediaItem.Type != MediaType.Video ? mediaItem.Url.Value : null,
            VideoUrl        = mediaItem.Type == MediaType.Video ? mediaItem.Url.Value : null,
            Url             = mediaItem.Url.Value,
            mediaItem.CoverImageUrl,
            mediaItem.CanvasUrl,
            mediaItem.Genre,
            DurationSeconds = mediaItem.Duration.TotalSeconds,
            TrailerSeconds  = mediaItem.DurationTrailer.TotalSeconds,
            DurationMinutes = mediaItem.Duration.Minutes,
            TrailerMinutes  = mediaItem.DurationTrailer.Minutes,
            AccessLevel     = (int)mediaItem.AccessLevel,
            mediaItem.IsPublic,
            mediaItem.IsActive,
            mediaItem.IsValid,
            mediaItem.FavoriteCount,
            mediaItem.ViewCount,
            mediaItem.UploadedAt,
            mediaItem.ReleaseDate
        }, cancellationToken: ct));
    }

    /// <summary>
    /// Thêm danh sách Artists liên kết với MediaItem vào bảng MediaArtists.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL INSERT với các cột: MediaItemId, ArtistId, Role
    /// 3. Lặp qua danh sách artists và INSERT từng bản ghi
    ///    - MediaItemId: Id của media
    ///    - ArtistId: Id của artist
    ///    - Role: vai trò của artist (singer, composer, producer, v.v.)
    /// 4. Thực hiện batch INSERT
    /// </summary>
    /// <param name="artists">Danh sách MediaArtist chứa liên kết giữa Media và Artists</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    public async Task AddArtistsAsync(IEnumerable<MediaArtist> artists, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO [MediaArtists] (MediaItemId, ArtistId, [Role])
            VALUES (@MediaItemId, @ArtistId, @Role)";

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, artists, cancellationToken: ct));
    }

    /// <summary>
    /// Cập nhật thông tin MediaItem (không cập nhật URL do đó là immutable).
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. Chuẩn bị SQL UPDATE với các cột: Title, Description, Genre, CoverImageUrl, CanvasUrl,
    ///    DurationSeconds, TrailerSeconds, AccessLevel, IsPublic, IsActive, FavoriteCount, ViewCount, ReleaseDate
    /// 3. WHERE Id = @Id để xác định media cần update
    /// 4. Trích xuất dữ liệu từ MediaItem entity sang parameters
    ///    - Duration: chuyển timespan sang seconds
    ///    - AccessLevel: chuyển enum thành int
    /// 5. Thực hiện UPDATE
    /// </summary>
    /// <param name="mediaItem">MediaItem entity chứa dữ liệu cập nhật</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    public async Task UpdateAsync(MediaItem mediaItem, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE [MediaItems]
            SET Title           = @Title,
                Description     = @Description,
                Genre           = @Genre,
                CoverImageUrl   = @CoverImageUrl,
                CanvasUrl       = @CanvasUrl,
                DurationSeconds = @DurationSeconds,
                TrailerSeconds  = @TrailerSeconds,
                AccessLevel     = @AccessLevel,
                IsPublic        = @IsPublic,
                IsActive        = @IsActive,
                IsValid         = @IsValid,
                FavoriteCount   = @FavoriteCount,
                ViewCount       = @ViewCount,
                ReleaseDate     = @ReleaseDate
            WHERE Id = @Id";

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            mediaItem.Id,
            mediaItem.Title,
            mediaItem.Description,
            mediaItem.Genre,
            mediaItem.CoverImageUrl,
            mediaItem.CanvasUrl,
            DurationSeconds = mediaItem.Duration.TotalSeconds,
            TrailerSeconds  = mediaItem.DurationTrailer.TotalSeconds,
            AccessLevel     = (int)mediaItem.AccessLevel,
            mediaItem.IsPublic,
            mediaItem.IsActive,
            mediaItem.IsValid,
            mediaItem.FavoriteCount,
            mediaItem.ViewCount,
            mediaItem.ReleaseDate
        }, cancellationToken: ct));
    }

    /// <summary>
    /// Deactivate MediaItem (soft delete) - đánh dấu media là không còn hoạt động.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database
    /// 2. UPDATE MediaItems SET IsActive = 0
    /// 3. WHERE Id = @Id AND IsActive = 1 (chỉ deactivate nếu đang active)
    /// 4. Lấy số rows bị ảnh hưởng
    /// 5. Trả về true nếu affected > 0 (deactivate thành công)
    /// </summary>
    /// <param name="id">Id của MediaItem cần deactivate</param>
    /// <param name="ct">CancellationToken để hủy operation</param>
    /// <returns>true nếu deactivate thành công, false nếu media không tồn tại hoặc đã inactive</returns>
    public async Task<bool> DeactivateAsync(string id, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE [MediaItems]
            SET IsActive = 0
            WHERE Id = @Id AND IsActive = 1";

        using var conn = _db.CreateConnection();
        var rowsAffected = await conn.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return rowsAffected > 0;
    }

    private static string GetContentType(string filePath, MediaType mediaType)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".flac" => "audio/flac",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "audio/ogg",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => mediaType == MediaType.Video ? "video/mp4" : "application/octet-stream"
        };
    }

    private sealed class MediaAssetRow
    {
        public string Id { get; init; } = string.Empty;
        public int MediaType { get; init; }
        public string? AudioUrl { get; init; }
        public string? VideoUrl { get; init; }
        public string? CoverImageUrl { get; init; }
    }

    /// <summary>
    /// Sinh mã định danh tuần tự tiếp theo cho MediaItem dạng M001, M002, ..., M999.
    /// 
    /// Các bước thực hiện:
    /// 1. Tạo connection đến database.
    /// 2. Lấy số lớn nhất từ các Id hiện có trong bảng MediaItems có định dạng 'M' + 3 chữ số.
    ///    - Sử dụng `MAX(CAST(SUBSTRING(Id, 2, 3) AS INT))` để lấy phần số.
    ///    - `ISNULL(..., 0) + 1` để xử lý trường hợp bảng trống hoặc bắt đầu từ 1.
    /// 3. Format số đó thành 3 chữ số có số 0 ở đầu (ví dụ: 1 -> "001", 12 -> "012", 123 -> "123").
    /// 4. Tiền tố 'M' vào số đã định dạng.
    /// 5. Trả về Id mới (ví dụ: "M001").
    /// </summary>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Id mới sinh ra (ví dụ: "M001").</returns>
    public async Task<string> GenerateNextMediaIdAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(Id, 2, 3) AS INT)), 0) + 1
            FROM [MediaItems]
            WHERE Id LIKE 'M[0-9][0-9][0-9]'"; // Ensure Id is M followed by exactly 3 digits
        using var conn = _db.CreateConnection();
        var nextNumber = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: ct));
        return $"M{nextNumber:D3}"; // M001, M002, ..., M999
    }
}
