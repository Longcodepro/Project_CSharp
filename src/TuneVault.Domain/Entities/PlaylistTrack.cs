using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một bài hát nằm trong Danh sách phát (PlaylistTrack) của hệ thống TuneVault.
/// Thực thể độc lập này chịu trách nhiệm quản lý liên kết và thứ tự sắp xếp của bài hát trong một Playlist cụ thể.
/// </summary>
public class PlaylistTrack
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MinTrackOrder = 1;
    private const int MaxTrackOrder = 100; // Playlist có sức chứa lớn hơn Album (Giới hạn tối đa 100 bài)

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của bản ghi PlaylistTrack. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh của Danh sách phát (Playlist) chứa bài hát này. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string PlaylistId { get; private set; }

    /// <summary>
    /// Mã định danh của vật phẩm phương tiện (MediaItem) được liên kết. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string MediaItemId { get; private set; }

    /// <summary>
    /// Vị trí/Thứ tự hiển thị và phát của bài hát bên trong Playlist (Giới hạn từ 1 đến 100).
    /// </summary>
    public int TrackOrder { get; private set; }

    /// <summary>
    /// Thời điểm bài hát được người dùng thêm vào Danh sách phát.
    /// </summary>
    public DateTime AddedAt { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private PlaylistTrack() { }

    /// <summary>
    /// Khởi tạo một thực thể PlaylistTrack mới với đầy đủ các ràng buộc nghiệp vụ khắt khe về định danh và vị trí.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của PlaylistTrack.</param>
    /// <param name="playlistId">Mã định danh của Playlist liên kết.</param>
    /// <param name="mediaItemId">Mã định danh của MediaItem liên kết.</param>
    /// <param name="trackOrder">Thứ tự sắp xếp của bài hát trong danh sách phát.</param>
    public PlaylistTrack(string id, string playlistId, string mediaItemId, int trackOrder)
    {
        ValidateId(id);
        ValidatePlaylistId(playlistId);
        ValidateMediaItemId(mediaItemId);
        ValidateTrackOrder(trackOrder);

        DateTime now = DateTime.UtcNow;
        ValidateAddedAt(now);

        Id = id.Trim();
        PlaylistId = playlistId.Trim();
        MediaItemId = mediaItemId.Trim();
        TrackOrder = trackOrder;
        AddedAt = now;
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật lại vị trí/thứ tự sắp xếp (TrackOrder) của bài hát khi người dùng thực hiện kéo thả đổi vị trí trong Playlist.
    /// </summary>
    /// <param name="newTrackOrder">Giá trị thứ tự mới cần thay đổi (Từ 1 đến 100).</param>
    public void UpdateTrackOrder(int newTrackOrder)
    {
        ValidateTrackOrder(newTrackOrder);
        TrackOrder = newTrackOrder;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh PlaylistTrack.
    /// </summary>
    /// <param name="id">Chuỗi định danh cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi Id trống hoặc không nằm trong khoảng từ 4 đến 5 ký tự.</exception>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của PlaylistTrack không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của PlaylistTrack phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh Danh sách phát (PlaylistId).
    /// </summary>
    /// <param name="playlistId">Chuỗi định danh Playlist cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi PlaylistId trống hoặc không nằm trong khoảng từ 4 đến 5 ký tự.</exception>
    private static void ValidatePlaylistId(string playlistId)
    {
        if (string.IsNullOrWhiteSpace(playlistId))
            throw new DomainException("PlaylistId không được để trống.");

        int length = playlistId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"PlaylistId phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh vật phẩm phương tiện (MediaItemId).
    /// </summary>
    /// <param name="mediaItemId">Chuỗi định danh MediaItem cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi MediaItemId trống hoặc không nằm trong khoảng từ 4 đến 5 ký tự.</exception>
    private static void ValidateMediaItemId(string mediaItemId)
    {
        if (string.IsNullOrWhiteSpace(mediaItemId))
            throw new DomainException("MediaItemId không được để trống.");

        int length = mediaItemId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"MediaItemId phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực vị trí thứ tự bài hát trong danh sách phát, giới hạn nghiêm ngặt từ 1 đến 100.
    /// </summary>
    /// <param name="trackOrder">Giá trị số nguyên đại diện thứ tự bài hát.</param>
    /// <exception cref="DomainException">Ném ra khi thứ tự nằm ngoài khoảng từ 1 đến 100.</exception>
    private static void ValidateTrackOrder(int trackOrder)
    {
        if (trackOrder < MinTrackOrder || trackOrder > MaxTrackOrder)
            throw new DomainException($"Thứ tự bài hát trong Playlist phải nằm trong khoảng từ {MinTrackOrder} đến {MaxTrackOrder}.");
    }

    /// <summary>
    /// Xác thực mốc thời gian hệ thống ghi nhận khi bài hát được thêm vào danh sách phát.
    /// </summary>
    /// <param name="addedAt">Mốc thời gian DateTime cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi mốc thời gian mang giá trị mặc định hoặc vượt quá thời gian hiện tại.</exception>
    private static void ValidateAddedAt(DateTime addedAt)
    {
        if (addedAt == default)
            throw new DomainException("Thời gian thêm bài hát (AddedAt) không được mang giá trị mặc định.");

        if (addedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian thêm bài hát không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}