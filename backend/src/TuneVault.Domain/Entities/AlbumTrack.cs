using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một bài hát nằm trong Album (AlbumTrack) của hệ thống TuneVault.
/// Thực thể này tự quản lý định danh dạng chuỗi và các quy tắc sắp xếp bài hát.
/// </summary>
public class AlbumTrack
{
    // --- Constants ---
    private const int MaxIdLength = 10;
    private const int MinTrackOrder = 1;
    private const int MaxTrackOrder = 20;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất của bản ghi AlbumTrack.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của Album chứa bài hát này.
    /// </summary>
    public string AlbumId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của vật phẩm phương tiện (MediaItem) được liên kết.
    /// </summary>
    public string MediaItemId { get; private set; } = string.Empty;

    /// <summary>
    /// Vị trí/Thứ tự phát của bài hát trong Album (Giới hạn từ 1 đến 20).
    /// </summary>
    public int TrackOrder { get; private set; }

    /// <summary>
    /// Thời điểm bài hát được thêm vào Album.
    /// </summary>
    public DateTime AddedAt { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private để phục vụ cơ chế mapping của Dapper/ORM.
    /// </summary>
    private AlbumTrack() { }

    /// <summary>
    /// Khởi tạo một thực thể AlbumTrack mới với các ràng buộc nghiệp vụ cố định về độ dài ID và giới hạn số lượng bài.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của AlbumTrack.</param>
    /// <param name="albumId">Mã định danh Album liên kết.</param>
    /// <param name="mediaItemId">Mã định danh MediaItem liên kết.</param>
    /// <param name="trackOrder">Thứ tự sắp xếp của bài hát trong Album (Từ 1 đến 20).</param>
    public AlbumTrack(string id, string albumId, string mediaItemId, int trackOrder)
    {
        ValidateId(id);
        ValidateAlbumId(albumId);
        ValidateMediaItemId(mediaItemId);
        ValidateTrackOrder(trackOrder);

        DateTime now = DateTime.UtcNow;
        ValidateAddedAt(now);

        Id = id.Trim();
        AlbumId = albumId.Trim();
        MediaItemId = mediaItemId.Trim();
        TrackOrder = trackOrder;
        AddedAt = now;
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật lại thứ tự sắp xếp (TrackOrder) của bài hát trong Album.
    /// </summary>
    /// <param name="newTrackOrder">Giá trị thứ tự mới cần thay đổi (Từ 1 đến 20).</param>
    public void UpdateTrackOrder(int newTrackOrder)
    {
        ValidateTrackOrder(newTrackOrder);
        TrackOrder = newTrackOrder;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ của mã định danh AlbumTrack.
    /// </summary>
    /// <param name="id">Chuỗi định danh cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi Id trống hoặc vượt quá giới hạn.</exception>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của AlbumTrack không được để trống.");

        if (id.Trim().Length > MaxIdLength)
            throw new DomainException($"Id của AlbumTrack không được vượt quá {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của mã định danh Album.
    /// </summary>
    /// <param name="albumId">Chuỗi định danh Album cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi AlbumId trống hoặc vượt quá giới hạn.</exception>
    private static void ValidateAlbumId(string albumId)
    {
        if (string.IsNullOrWhiteSpace(albumId))
            throw new DomainException("AlbumId không được để trống.");

        if (albumId.Trim().Length > MaxIdLength)
            throw new DomainException($"AlbumId không được vượt quá {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của mã định danh vật phẩm phương tiện (MediaItemId).
    /// </summary>
    /// <param name="mediaItemId">Chuỗi định danh MediaItem cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi MediaItemId trống hoặc vượt quá giới hạn.</exception>
    private static void ValidateMediaItemId(string mediaItemId)
    {
        if (string.IsNullOrWhiteSpace(mediaItemId))
            throw new DomainException("MediaItemId không được để trống.");

        if (mediaItemId.Trim().Length > MaxIdLength)
            throw new DomainException($"MediaItemId không được vượt quá {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực vị trí thứ tự bài hát trong Album, giới hạn nghiêm ngặt từ 1 đến 20.
    /// </summary>
    /// <param name="trackOrder">Giá trị số nguyên đại diện thứ tự bài hát.</param>
    /// <exception cref="DomainException">Ném ra khi thứ tự nằm ngoài khoảng từ 1 đến 20.</exception>
    private static void ValidateTrackOrder(int trackOrder)
    {
        if (trackOrder < MinTrackOrder || trackOrder > MaxTrackOrder)
            throw new DomainException($"Thứ tự bài hát (TrackOrder) phải nằm trong khoảng từ {MinTrackOrder} đến {MaxTrackOrder}.");
    }

    /// <summary>
    /// Xác thực mốc thời gian hệ thống tạo bản ghi.
    /// </summary>
    /// <param name="addedAt">Mốc thời gian DateTime cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi mốc thời gian không hợp lệ hoặc thuộc về tương lai phi lý.</exception>
    private static void ValidateAddedAt(DateTime addedAt)
    {
        if (addedAt == default)
            throw new DomainException("Thời gian thêm bài hát (AddedAt) không được mang giá trị mặc định.");

        if (addedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian thêm bài hát không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}
