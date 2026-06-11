namespace TuneVault.Domain.Interfaces;

/// <summary>
/// SUMMARY PHẦN TÌM KIẾM & KHÁM PHÁ - DOMAIN INTERFACE
/// File này khai báo các chức năng search/discovery cần có.
/// 
/// Nhiệm vụ được cover:
/// - Tìm kiếm bài hát / podcast.
/// - Tìm kiếm nghệ sĩ.
/// - Tìm kiếm album.
/// - Tìm kiếm playlist.
/// - Lọc theo thể loại / genre.
/// - Lấy danh sách trending, tức bài nghe nhiều nhất.
/// 
/// Lưu ý:
/// Interface chỉ khai báo method.
/// SQL thật nằm ở Infrastructure/Repositories/SearchRepository.cs.
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// Tìm kiếm bài hát / podcast theo keyword.
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
    /// Lọc bài hát / podcast theo genre.
    /// </summary>
    Task<IEnumerable<dynamic>> SearchByGenreAsync(string genre, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách bài nghe nhiều nhất.
    /// </summary>
    Task<IEnumerable<dynamic>> GetTrendingAsync(int top = 10, CancellationToken cancellationToken = default);
}
