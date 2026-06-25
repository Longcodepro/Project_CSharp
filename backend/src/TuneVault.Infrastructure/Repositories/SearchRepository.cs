using Dapper;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Persistence;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// IMPLEMENTATION - SEARCH REPOSITORY (Infrastructure Layer)
/// =========================================================
/// Mục đích: Cài đặt toàn bộ chức năng tìm kiếm và khám phá dữ liệu dùng Dapper.
/// 
/// Luồng xử lý:
/// 1. SearchController gọi ISearchRepository (injected)
/// 2. DI container trỏ tới SearchRepository
/// 3. Repository mở connection từ IDbConnectionFactory
/// 4. Repository thực thi SQL và trả về kết quả dynamic
/// 
/// Các chức năng:
/// - SearchMediaAsync: Tìm bài hát/podcast theo title, JOIN MediaArtists+Users để lấy ArtistName
/// - SearchArtistsAsync: Tìm nghệ sĩ theo IdDisplay hoặc DisplayName
/// - SearchAlbumsAsync: Tìm album công khai theo title
/// - SearchPlaylistsAsync: Tìm playlist công khai theo title, JOIN Users lấy OwnerName, đếm TrackCount
/// - GetTrendingAsync: Lấy top media theo ViewCount
/// </summary>
public sealed class SearchRepository : ISearchRepository
{
    private readonly IDbConnectionFactory _db;

    /// <summary>
    /// Khởi tạo SearchRepository với IDbConnectionFactory dependency.
    /// </summary>
    /// <param name="db">Factory để tạo kết nối database.</param>
    public SearchRepository(IDbConnectionFactory db) => _db = db;

    /// <summary>
    /// Tìm kiếm bài hát / podcast theo title.
    /// JOIN MediaArtists + Users để lấy ArtistName (DisplayName của nghệ sĩ đầu tiên)
    /// </summary>
    public async Task<IEnumerable<dynamic>> SearchMediaAsync(string keyword, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                m.Id,
                m.Title,
                u.DisplayName AS ArtistName,
                m.Genre,
                m.DurationSeconds,
                m.ViewCount,
                m.CoverImageUrl
            FROM MediaItems m
            LEFT JOIN MediaArtists ma ON m.Id = ma.MediaItemId
            LEFT JOIN Users u ON ma.ArtistId = u.Id
            WHERE m.IsPublic = 1
              AND m.IsActive = 1
              AND m.IsValid = 0
              AND (m.Title LIKE @Keyword OR u.DisplayName LIKE @Keyword)
            ORDER BY m.Title ASC;";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            new CommandDefinition(sql, new { Keyword = ToLikeKeyword(keyword) }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Tìm kiếm nghệ sĩ theo IdDisplay hoặc DisplayName.
    /// SQL: SELECT Id, IdDisplay AS UserName, DisplayName, AvatarUrl, TotalFollowers FROM Users
    /// </summary>
    public async Task<IEnumerable<dynamic>> SearchArtistsAsync(string keyword, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                Id,
                IdDisplay AS UserName,
                DisplayName,
                AvatarUrl,
                TotalFollowers
            FROM Users
            WHERE IsActive = 1
              AND IsArtist = 1
              AND (IdDisplay LIKE @Keyword OR DisplayName LIKE @Keyword)
            ORDER BY DisplayName ASC;";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            new CommandDefinition(sql, new { Keyword = ToLikeKeyword(keyword) }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Tìm kiếm album công khai theo title.
    /// SQL: SELECT * FROM Albums WHERE IsPublic = 1 AND Title LIKE @Keyword ORDER BY ReleaseDate DESC, Title ASC
    /// </summary>
    public async Task<IEnumerable<dynamic>> SearchAlbumsAsync(string keyword, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                Id,
                ArtistId,
                Title,
                Description,
                CoverImageUrl,
                CreatedAt
            FROM Albums
            WHERE IsActive = 1
              AND IsPublic = 1
              AND Title LIKE @Keyword
            ORDER BY CreatedAt DESC, Title ASC;";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            new CommandDefinition(sql, new { Keyword = ToLikeKeyword(keyword) }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Tìm kiếm playlist công khai theo title.
    /// JOIN Users để lấy OwnerName (DisplayName), JOIN PlaylistTracks để đếm TrackCount
    /// </summary>
    public async Task<IEnumerable<dynamic>> SearchPlaylistsAsync(string keyword, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                p.Id,
                p.Title,
                p.CoverImageUrl,
                u.DisplayName AS OwnerName,
                COUNT(pt.MediaItemId) AS TrackCount,
                p.CreatedAt
            FROM Playlists p
            LEFT JOIN Users u ON p.UserId = u.Id
            LEFT JOIN PlaylistTracks pt ON p.Id = pt.PlaylistId
            WHERE p.IsPublic = 1
              AND p.IsActive = 1
              AND p.Title LIKE @Keyword
            GROUP BY p.Id, p.Title, p.CoverImageUrl, u.DisplayName, p.CreatedAt
            ORDER BY p.CreatedAt DESC, p.Title ASC;";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            new CommandDefinition(sql, new { Keyword = ToLikeKeyword(keyword) }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Lấy top bài nghe nhiều nhất dựa theo ViewCount.
    /// SQL: SELECT TOP (@Top) ... FROM MediaItems WHERE IsPublic = 1 ORDER BY ViewCount DESC
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetTrendingAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP (@Top)
                m.Id,
                m.Title,
                u.DisplayName AS ArtistName,
                m.Genre,
                m.DurationSeconds,
                m.ViewCount,
                m.CoverImageUrl
            FROM MediaItems m
            LEFT JOIN MediaArtists ma ON m.Id = ma.MediaItemId
            LEFT JOIN Users u ON ma.ArtistId = u.Id
            WHERE m.IsPublic = 1
              AND m.IsActive = 1
              AND m.IsValid = 0
            ORDER BY m.ViewCount DESC;";

        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            new CommandDefinition(sql, new { Top = Math.Clamp(top, 1, 50) }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Chuyển keyword thường thành keyword dạng LIKE của SQL.
    /// Ví dụ: love -> %love%
    /// </summary>
    private static string ToLikeKeyword(string keyword)
        => $"%{keyword.Trim()}%";
}
