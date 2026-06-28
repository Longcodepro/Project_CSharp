namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Khai báo các truy vấn tìm kiếm và khám phá nội dung.
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// Tìm kiếm media theo keyword.
    /// </summary>
    Task<IEnumerable<dynamic>> SearchMediaAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm nghệ sĩ theo keyword.
    /// </summary>
    Task<IEnumerable<dynamic>> SearchArtistsAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm album theo keyword.
    /// </summary>
    Task<IEnumerable<dynamic>> SearchAlbumsAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm playlist theo keyword.
    /// </summary>
    Task<IEnumerable<dynamic>> SearchPlaylistsAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách bài nghe nhiều nhất.
    /// </summary>
    Task<IEnumerable<dynamic>> GetTrendingAsync(int top = 10, CancellationToken cancellationToken = default);
}
