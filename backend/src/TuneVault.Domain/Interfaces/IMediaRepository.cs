using TuneVault.Domain.Entities;
using TuneVault.Domain.Enums;
using TuneVault.Domain.ValueObject;

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
    /// Lấy thông tin media công khai theo Id cho endpoint public.
    /// </summary>
    /// <param name="id">Mã định danh media.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Media public, active, không bị khóa hoặc <c>null</c>.</returns>
    Task<MediaItem?> GetPublicByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Lấy tên hiển thị của người sở hữu media.
    /// </summary>
    /// <param name="mediaItemId">Mã định danh bài hát.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>Tên hiển thị của owner hoặc <c>null</c> nếu không tìm thấy.</returns>
    Task<string?> GetOwnerDisplayNameAsync(string mediaItemId, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách media theo phân trang.
    /// </summary>
    Task<IReadOnlyCollection<MediaItem>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách media của một owner.
    /// </summary>
    Task<IReadOnlyCollection<MediaItem>> GetByOwnerAsync(string ownerId, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách media công khai của một owner để phục vụ người xem bên ngoài.
    /// </summary>
    Task<IReadOnlyCollection<MediaItem>> GetPublicByOwnerAsync(string ownerId, CancellationToken ct = default);

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

    /// <summary>
    /// Lấy thông tin stream của một MediaItem theo Id.
    /// </summary>
    /// <param name="mediaId">Mã định danh media (VD: I001).</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    Task<MediaStreamInfo?> GetStreamAsync(
        string mediaId,
        string? requesterId = null,
        MediaAssetKind assetKind = MediaAssetKind.Primary,
        CancellationToken ct = default);
}
