using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Interface kho dữ liệu cho <see cref="MediaItem"/>.
/// Định nghĩa các thao tác truy vấn và ghi dữ liệu liên quan đến bài hát/media.
/// Tất cả Id sử dụng kiểu <c>string</c> (VD: I001) thống nhất với toàn bộ hệ thống.
/// </summary>
public interface IMediaRepository
{
    // =========================================================================
    // QUERIES
    // =========================================================================

    /// <summary>
    /// Lấy thông tin một MediaItem theo Id nội bộ.
    /// </summary>
    /// <param name="id">Mã định danh nội bộ (VD: I001).</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Entity <see cref="MediaItem"/> hoặc <c>null</c> nếu không tồn tại hoặc đã bị xóa.</returns>
    Task<MediaItem?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách các <see cref="MediaArtist"/> của một bài hát (ca sĩ chính + ca sĩ phụ).
    /// </summary>
    /// <param name="mediaItemId">Mã định danh bài hát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách <see cref="MediaArtist"/>.</returns>
    Task<IEnumerable<MediaArtist>> GetArtistsByMediaIdAsync(string mediaItemId, CancellationToken ct = default);

    /// <summary>
    /// Tìm kiếm bài hát theo từ khóa (title, genre, artist name).
    /// Chỉ trả về bài hát đang IsActive = true.
    /// </summary>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Danh sách <see cref="MediaItem"/> khớp với từ khóa.</returns>
    Task<IEnumerable<MediaItem>> SearchAsync(string keyword, CancellationToken ct = default);

    /// <summary>
    /// Sinh mã định danh tuần tự tiếp theo cho MediaItem (VD: I001, I002...).
    /// </summary>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Mã định danh dạng string (VD: I005).</returns>
    Task<string> GenerateNextMediaIdAsync(CancellationToken ct = default);

    // =========================================================================
    // COMMANDS
    // =========================================================================

    /// <summary>
    /// Thêm một <see cref="MediaItem"/> mới vào database.
    /// </summary>
    /// <param name="mediaItem">Entity bài hát cần lưu.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task AddAsync(MediaItem mediaItem, CancellationToken ct = default);

    /// <summary>
    /// Thêm danh sách quan hệ nghệ sĩ cho một bài hát (bulk insert).
    /// Mỗi bài hát có 1 MainArtist và n FeaturedArtist.
    /// </summary>
    /// <param name="artists">Danh sách <see cref="MediaArtist"/> cần thêm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task AddArtistsAsync(IEnumerable<MediaArtist> artists, CancellationToken ct = default);

    /// <summary>
    /// Cập nhật thông tin của một <see cref="MediaItem"/> trong database.
    /// </summary>
    /// <param name="mediaItem">Entity bài hát với các thông tin đã thay đổi qua method nghiệp vụ.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task UpdateAsync(MediaItem mediaItem, CancellationToken ct = default);

    /// <summary>
    /// Thực hiện Soft Delete — chuyển <c>IsActive = false</c> cho bài hát.
    /// Không xóa bản ghi vật lý khỏi database.
    /// </summary>
    /// <param name="id">Mã định danh bài hát cần vô hiệu hóa.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu thao tác thành công.</returns>
    Task<bool> DeactivateAsync(string id, CancellationToken ct = default);
}
