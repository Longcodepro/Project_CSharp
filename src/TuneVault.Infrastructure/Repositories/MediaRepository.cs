using Dapper;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Triển khai <see cref="IMediaRepository"/> bằng Dapper + SQL Server.
/// Chịu trách nhiệm toàn bộ truy vấn và thao tác dữ liệu liên quan đến <see cref="MediaItem"/>.
/// </summary>
public sealed class MediaRepository : IMediaRepository
{
    private readonly DapperContext _context;

    /// <summary>
    /// Khởi tạo repository với <see cref="DapperContext"/> được inject qua DI container.
    /// </summary>
    /// <param name="context">Wrapper context chứa factory tạo kết nối SQL Server.</param>
    public MediaRepository(DapperContext context)
    {
        _context = context;
    }

    // =========================================================================
    // QUERIES
    // =========================================================================

    /// <summary>
    /// Lấy thông tin một bài hát theo Id nội bộ.
    /// Chỉ trả về bài hát đang IsActive = true.
    /// </summary>
    /// <param name="id">Mã định danh nội bộ (VD: I001).</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="MediaItem"/> hoặc <c>null</c> nếu không tồn tại hoặc đã bị xóa.</returns>
    public async Task<MediaItem?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        // Step 1: Định nghĩa câu truy vấn SELECT theo PK và IsActive
        const string sql = "SELECT * FROM [MediaItems] WHERE Id = @Id AND IsActive = 1";

        // Step 2: Mở connection và thực thi truy vấn
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<MediaItem>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    /// <summary>
    /// Lấy danh sách nghệ sĩ liên quan đến một bài hát (MainArtist + FeaturedArtist).
    /// </summary>
    /// <param name="mediaItemId">Mã định danh bài hát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách <see cref="MediaArtist"/>.</returns>
    public async Task<IEnumerable<MediaArtist>> GetArtistsByMediaIdAsync(string mediaItemId, CancellationToken ct = default)
    {
        // Step 1: Lấy toàn bộ row trong MediaArtists có MediaItemId khớp
        const string sql = "SELECT * FROM [MediaArtists] WHERE MediaItemId = @MediaItemId";

        // Step 2: Mở connection và thực thi truy vấn
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<MediaArtist>(
            new CommandDefinition(sql, new { MediaItemId = mediaItemId }, cancellationToken: ct));
    }

    /// <summary>
    /// Tìm kiếm bài hát theo từ khóa trong title hoặc genre.
    /// Chỉ tìm bài hát đang IsActive = true và IsPublic = true.
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách <see cref="MediaItem"/> khớp với từ khóa.</returns>
    public async Task<IEnumerable<MediaItem>> SearchAsync(string keyword, CancellationToken ct = default)
    {
        // Step 1: Tìm kiếm LIKE theo Title và Genre — tối ưu với NONCLUSTERED INDEX đã có
        const string sql = @"
            SELECT * FROM [MediaItems]
            WHERE IsActive = 1 AND IsPublic = 1
              AND (Title LIKE @Keyword OR Genre LIKE @Keyword)
            ORDER BY UploadedAt DESC";

        // Step 2: Bọc keyword trong dấu % cho LIKE pattern
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<MediaItem>(
            new CommandDefinition(sql, new { Keyword = $"%{keyword}%" }, cancellationToken: ct));
    }

    // =========================================================================
    // COMMANDS
    // =========================================================================

    /// <summary>
    /// Thêm một <see cref="MediaItem"/> mới vào database.
    /// </summary>
    /// <param name="mediaItem">Entity bài hát cần lưu.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    public async Task AddAsync(MediaItem mediaItem, CancellationToken ct = default)
    {
        // Step 1: INSERT toàn bộ cột vào bảng MediaItems
        // DurationSeconds được map từ Duration.TotalSeconds (computed từ Value Object)
        const string sql = @"
            INSERT INTO [MediaItems]
                (Id, OwnerId, Title, Description, MediaType, AudioUrl, VideoUrl,
                 CoverImageUrl, CanvasUrl, Genre, DurationSeconds, TrailerSeconds,
                 AccessLevel, IsPublic, IsActive, FavoriteCount, ViewCount, UploadedAt, ReleaseDate)
            VALUES
                (@Id, @OwnerId, @Title, @Description, @MediaType, @AudioUrl, @VideoUrl,
                 @CoverImageUrl, @CanvasUrl, @Genre, @DurationSeconds, @TrailerSeconds,
                 @AccessLevel, @IsPublic, @IsActive, @FavoriteCount, @ViewCount, @UploadedAt, @ReleaseDate)";

        // Step 2: Map các property của Entity sang anonymous object (tránh expose trực tiếp)
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            mediaItem.Id,
            mediaItem.OwnerId,
            mediaItem.Title,
            mediaItem.Description,
            MediaType        = (int)mediaItem.Type,
            AudioUrl         = mediaItem.Type != Domain.Enums.MediaType.Video ? mediaItem.Url.Value : null,
            VideoUrl         = mediaItem.Type == Domain.Enums.MediaType.Video ? mediaItem.Url.Value : null,
            mediaItem.CoverImageUrl,
            mediaItem.CanvasUrl,
            mediaItem.Genre,
            DurationSeconds  = mediaItem.Duration.TotalSeconds,
            TrailerSeconds   = mediaItem.DurationTrailer.TotalSeconds,
            AccessLevel      = (int)mediaItem.AccessLevel,
            mediaItem.IsPublic,
            mediaItem.IsActive,
            mediaItem.FavoriteCount,
            mediaItem.ViewCount,
            mediaItem.UploadedAt,
            mediaItem.ReleaseDate
        }, cancellationToken: ct));
    }

    /// <summary>
    /// Thêm danh sách quan hệ nghệ sĩ cho bài hát (bulk insert).
    /// </summary>
    /// <param name="artists">Danh sách <see cref="MediaArtist"/> cần insert.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    public async Task AddArtistsAsync(IEnumerable<MediaArtist> artists, CancellationToken ct = default)
    {
        // Step 1: INSERT từng nghệ sĩ vào bảng MediaArtists (composite PK: MediaItemId + ArtistId)
        const string sql = @"
            INSERT INTO [MediaArtists] (MediaItemId, ArtistId, [Role])
            VALUES (@MediaItemId, @ArtistId, @Role)";

        // Step 2: Dapper sẽ tự loop danh sách và bulk insert
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, artists, cancellationToken: ct));
    }

    /// <summary>
    /// Cập nhật thông tin của một <see cref="MediaItem"/> trong database.
    /// </summary>
    /// <param name="mediaItem">Entity bài hát với thông tin đã thay đổi.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    public async Task UpdateAsync(MediaItem mediaItem, CancellationToken ct = default)
    {
        // Step 1: UPDATE toàn bộ các cột có thể thay đổi — không cập nhật Id, OwnerId, UploadedAt
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
                FavoriteCount   = @FavoriteCount,
                ViewCount       = @ViewCount,
                ReleaseDate     = @ReleaseDate
            WHERE Id = @Id";

        // Step 2: Thực thi UPDATE
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
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
            mediaItem.FavoriteCount,
            mediaItem.ViewCount,
            mediaItem.ReleaseDate
        }, cancellationToken: ct));
    }

    /// <summary>
    /// Thực hiện Soft Delete — chuyển <c>IsActive = false</c> trực tiếp qua SQL.
    /// Nhanh hơn UpdateAsync vì chỉ cập nhật đúng 1 cột.
    /// </summary>
    /// <param name="id">Mã định danh bài hát cần vô hiệu hóa.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu có ít nhất 1 row bị ảnh hưởng.</returns>
    public async Task<bool> DeactivateAsync(string id, CancellationToken ct = default)
    {
        // Step 1: UPDATE chỉ cột IsActive — tối ưu hơn UpdateAsync toàn bộ row
        // Step 2: Thêm điều kiện IsActive = 1 để tránh UPDATE bản ghi đã soft-deleted
        const string sql = @"
            UPDATE [MediaItems]
            SET IsActive = 0
            WHERE Id = @Id AND IsActive = 1";

        // Step 3: Thực thi và kiểm tra rowsAffected
        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return rowsAffected > 0;
    }
}
