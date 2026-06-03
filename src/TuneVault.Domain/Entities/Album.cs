using System;
using System.Collections.Generic;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một Album trong hệ thống TuneVault.
/// Quản lý thông tin album, trạng thái và các quy tắc nghiệp vụ khắt khe.
/// Mọi định danh đều sử dụng kiểu chuỗi (string).
/// </summary>
public class Album
{
    // --- Constants ---
    private const int MaxIdLength = 4;
    private const int MaxTitleLength = 24;
    private const int MaxDescriptionLength = 500;
    private const int MaxUrlLength = 2048;

    // --- Fields ---
    private readonly List<AlbumTrack> _tracks = new();

    // --- Properties ---

    /// <summary>
    /// Mã định danh nghiệp vụ (Primary Key) của Album.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh của nghệ sĩ sở hữu Album.
    /// </summary>
    public string ArtistId { get; private set; }

    /// <summary>
    /// Tiêu đề của Album.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Mô tả chi tiết về Album.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Đường dẫn hợp lệ trỏ tới ảnh bìa của Album.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Thời điểm khởi tạo Album trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Danh sách các bài hát nằm trong Album này (Chỉ đọc để đảm bảo tính đóng gói).
    /// </summary>
    public IReadOnlyCollection<AlbumTrack> Tracks => _tracks.AsReadOnly();

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM.
    /// </summary>
    private Album() { }

    /// <summary>
    /// Khởi tạo một Album mới với các thông tin đầy đủ.
    /// </summary>
    /// <param name="id">Mã định danh Album.</param>
    /// <param name="artistId">Mã định danh nghệ sĩ.</param>
    /// <param name="title">Tiêu đề Album.</param>
    /// <param name="description">Mô tả Album (tùy chọn).</param>
    /// <param name="coverImageUrl">Đường dẫn ảnh bìa (tùy chọn).</param>
    public Album(string id, string artistId, string title, string? description = null, string? coverImageUrl = null)
    {
        ValidateId(id);
        ValidateArtistId(artistId);
        ValidateTitle(title);
        ValidateDescription(description);
        ValidateCoverImageUrl(coverImageUrl);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        ArtistId = artistId.Trim();
        Title = title.Trim();
        Description = description?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        CreatedAt = now;
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật tiêu đề của Album.
    /// </summary>
    /// <param name="title">Tiêu đề mới.</param>
    public void Rename(string title)
    {
        ValidateTitle(title);
        Title = title.Trim();
    }

    /// <summary>
    /// Cập nhật mô tả cho Album.
    /// </summary>
    /// <param name="description">Nội dung mô tả mới.</param>
    public void UpdateDescription(string? description)
    {
        ValidateDescription(description);
        Description = description?.Trim();
    }

    /// <summary>
    /// Cập nhật đường dẫn ảnh bìa của Album.
    /// </summary>
    /// <param name="coverImageUrl">Đường dẫn URL ảnh bìa mới.</param>
    public void UpdateCoverImage(string? coverImageUrl)
    {
        ValidateCoverImageUrl(coverImageUrl);
        CoverImageUrl = coverImageUrl?.Trim();
    }

    // --- Validation Methods ---

    /// <summary>
    /// Kiểm tra tính hợp lệ của Id Album.
    /// </summary>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id Album không được để trống.");
            
        if (id.Trim().Length > MaxIdLength)
            throw new DomainException($"Id Album không được vượt quá {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của ArtistId.
    /// </summary>
    private static void ValidateArtistId(string artistId)
    {
        if (string.IsNullOrWhiteSpace(artistId))
            throw new DomainException("ArtistId không được để trống.");
            
        if (artistId.Trim().Length > MaxIdLength)
            throw new DomainException($"ArtistId không được vượt quá {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của tiêu đề Album.
    /// </summary>
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề Album không được để trống.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề Album không được vượt quá {MaxTitleLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mô tả Album.
    /// </summary>
    private static void ValidateDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > MaxDescriptionLength)
            throw new DomainException($"Mô tả Album không được vượt quá {MaxDescriptionLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của URL ảnh bìa, đảm bảo đúng định dạng web.
    /// </summary>
    private static void ValidateCoverImageUrl(string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(coverImageUrl)) return;

        string trimmedUrl = coverImageUrl.Trim();
        if (trimmedUrl.Length > MaxUrlLength)
            throw new DomainException($"Đường dẫn ảnh bìa không được vượt quá {MaxUrlLength} ký tự.");

        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out Uri? uriResult) || 
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("Đường dẫn ảnh bìa không hợp lệ (phải là định dạng HTTP/HTTPS).");
        }
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mốc thời gian tạo.
    /// </summary>
    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo (CreatedAt) không hợp lệ.");
            
        // Đảm bảo thời gian không bị đặt thành một mốc thời gian phi lý trong tương lai xa
        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo không được vượt quá thời gian hiện tại.");
    }

    // --- Aggregate Methods (thêm mới) ---

    /// <summary>
    /// Thêm một bài hát mới (AlbumTrack) vào Album.
    /// Bắt buộc thêm qua Root để đảm bảo tính toàn vẹn của Aggregate.
    /// </summary>
    /// <param name="track">Thực thể AlbumTrack cần thêm vào Album.</param>
    /// <exception cref="DomainException">Ném ra khi track là null hoặc track không thuộc Album này.</exception>
    public void AddTrack(AlbumTrack track)
    {
        if (track is null)
            throw new DomainException("Bài hát được thêm vào Album không được phép rỗng (null).");

        if (track.AlbumId != Id)
            throw new DomainException("Bài hát không thuộc về Album này.");

        _tracks.Add(track);
    }

    /// <summary>
    /// Xóa một bài hát khỏi Album theo mã định danh của AlbumTrack.
    /// Bắt buộc xóa qua Root để đảm bảo tính toàn vẹn của Aggregate.
    /// </summary>
    /// <param name="trackId">Mã định danh của AlbumTrack cần xóa.</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát với trackId tương ứng trong Album.</exception>
    public void RemoveTrack(string trackId)
    {
        AlbumTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Album này.");

        _tracks.Remove(track);
    }

    /// <summary>
    /// Cập nhật thứ tự sắp xếp của một bài hát trong Album.
    /// Bắt buộc thực hiện qua Root để đảm bảo tính toàn vẹn của Aggregate.
    /// </summary>
    /// <param name="trackId">Mã định danh của AlbumTrack cần cập nhật thứ tự.</param>
    /// <param name="newTrackOrder">Giá trị thứ tự mới (từ 1 đến 20).</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát với trackId tương ứng trong Album.</exception>
    public void UpdateTrackOrder(string trackId, int newTrackOrder)
    {
        AlbumTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Album này.");

        track.UpdateTrackOrder(newTrackOrder);
    }
}