using System;
using System.Collections.Generic;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một Danh sách phát (Playlist) trong hệ thống TuneVault.
/// Thực thể độc lập này tự quản lý thông tin, ảnh bìa, danh sách bài hát đi kèm và các quy tắc nghiệp vụ liên quan.
/// </summary>
public class Playlist
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MaxTitleLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MaxUrlLength = 2048;

    // --- Fields ---
    private readonly List<PlaylistTrack> _tracks = new();

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của Playlist. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh của người dùng (User) sở hữu Playlist này. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Tiêu đề hiển thị của Danh sách phát.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Mô tả chi tiết về nội dung hoặc chủ đề của Playlist.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Đường dẫn liên kết (URL) trỏ tới ảnh bìa hoặc ảnh nền của Playlist.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Trạng thái hiển thị công khai (True) hoặc riêng tư (False) của Playlist.
    /// </summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// Thời điểm khởi tạo Playlist trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Danh sách các bài hát nằm trong Playlist này (Chỉ đọc để đảm bảo tính đóng gói).
    /// </summary>
    public IReadOnlyCollection<PlaylistTrack> Tracks => _tracks.AsReadOnly();

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private Playlist() { }

    /// <summary>
    /// Khởi tạo một Playlist mới với các thông tin ràng buộc và kiểm tra dữ liệu nghiêm ngặt.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của Playlist.</param>
    /// <param name="userId">Mã định danh của người dùng sở hữu.</param>
    /// <param name="title">Tiêu đề của Playlist.</param>
    /// <param name="description">Mô tả Playlist (tùy chọn).</param>
    /// <param name="coverImageUrl">Đường dẫn URL ảnh bìa hoặc ảnh nền (tùy chọn).</param>
    /// <param name="isPublic">Trạng thái công khai công bố (mặc định là false).</param>
    public Playlist(string id, string userId, string title, string? description = null, string? coverImageUrl = null, bool isPublic = false)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateTitle(title);
        ValidateDescription(description);
        ValidateCoverImageUrl(coverImageUrl);
        ValidateIsPublic(isPublic);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        UserId = userId.Trim();
        Title = title.Trim();
        Description = description?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        IsPublic = isPublic;
        CreatedAt = now;
    }

    // --- Business Methods ---

    /// <summary>
    /// Thực hiện đổi tên (Tiêu đề) của Playlist.
    /// </summary>
    /// <param name="title">Tiêu đề mới cần cập nhật.</param>
    public void Rename(string title)
    {
        ValidateTitle(title);
        Title = title.Trim();
    }

    /// <summary>
    /// Thực hiện cập nhật nội dung mô tả của Playlist.
    /// </summary>
    /// <param name="description">Nội dung mô tả mới.</param>
    public void UpdateDescription(string? description)
    {
        ValidateDescription(description);
        Description = description?.Trim();
    }

    /// <summary>
    /// Thực hiện cập nhật hoặc thay đổi đường dẫn ảnh bìa/ảnh nền của Playlist.
    /// </summary>
    /// <param name="coverImageUrl">Đường dẫn URL ảnh nền mới.</param>
    public void UpdateCoverImage(string? coverImageUrl)
    {
        ValidateCoverImageUrl(coverImageUrl);
        CoverImageUrl = coverImageUrl?.Trim();
    }

    /// <summary>
    /// Thay đổi trạng thái hiển thị công khai hoặc riêng tư của Playlist.
    /// </summary>
    /// <param name="isPublic">Giá trị trạng thái hiển thị mới.</param>
    public void SetPublic(bool isPublic)
    {
        ValidateIsPublic(isPublic);
        IsPublic = isPublic;
    }

    /// <summary>
    /// Thêm một bài hát mới (PlaylistTrack) vào danh sách phát.
    /// </summary>
    /// <param name="track">Thực thể bản ghi bài hát cần thêm.</param>
    public void AddTrack(PlaylistTrack track)
    {
        ValidateTrack(track);
        _tracks.Add(track);
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh Playlist.
    /// </summary>
    /// <param name="id">Chuỗi định danh cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi Id trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id Playlist không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id Playlist phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh người dùng (UserId).
    /// </summary>
    /// <param name="userId">Chuỗi định danh người dùng cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi UserId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của tiêu đề Playlist.
    /// </summary>
    /// <param name="title">Tiêu đề cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi tiêu đề trống hoặc vượt quá độ dài tối đa cho phép.</exception>
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề Playlist không được để trống.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề Playlist không được vượt quá {MaxTitleLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của phần mô tả Playlist.
    /// </summary>
    /// <param name="description">Chuỗi nội dung mô tả cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi nội dung vượt quá giới hạn độ dài cho phép.</exception>
    private static void ValidateDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > MaxDescriptionLength)
            throw new DomainException($"Mô tả Playlist không được vượt quá {MaxDescriptionLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của URL ảnh bìa hoặc ảnh nền, đảm bảo cấu trúc giao thức web HTTP/HTTPS.
    /// </summary>
    /// <param name="coverImageUrl">Đường dẫn liên kết cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi URL vượt quá giới hạn ký tự hoặc sai định dạng web tuyệt đối.</exception>
    private static void ValidateCoverImageUrl(string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(coverImageUrl)) return;

        string trimmedUrl = coverImageUrl.Trim();
        if (trimmedUrl.Length > MaxUrlLength)
            throw new DomainException($"Đường dẫn ảnh bìa Playlist không được vượt quá {MaxUrlLength} ký tự.");

        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out Uri? uriResult) || 
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("Đường dẫn ảnh bìa không hợp lệ (phải bắt đầu bằng định dạng HTTP hoặc HTTPS).");
        }
    }

    /// <summary>
    /// Xác thực tính hợp lệ của trạng thái công khai.
    /// </summary>
    /// <param name="isPublic">Giá trị logic hiển thị công khai.</param>
    private static void ValidateIsPublic(bool isPublic)
    {
        // Duy trì phương thức tuân thủ quy tắc 100% thuộc tính có hàm validate riêng biệt.
    }

    /// <summary>
    /// Xác thực tính toàn vẹn của mốc thời gian hệ thống ghi nhận khởi tạo Playlist.
    /// </summary>
    /// <param name="createdAt">Mốc thời gian cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi mốc thời gian mang giá trị mặc định hoặc thuộc về tương lai phi lý.</exception>
    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo Playlist không được mang giá trị mặc định.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo Playlist không hợp lệ (không được vượt quá thời gian hiện tại).");
    }

    /// <summary>
    /// Xác thực đối tượng bài hát trước khi thêm vào Playlist.
    /// </summary>
    /// <param name="track">Thực thể bài hát cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi thực thể bài hát truyền vào bị null.</exception>
    private static void ValidateTrack(PlaylistTrack track)
    {
        if (track == null)
            throw new DomainException("Bài hát được thêm vào Playlist không được phép rỗng (null).");
    }

    // --- Aggregate Methods (thêm mới) ---

    /// <summary>
    /// Xóa một bài hát khỏi Playlist theo mã định danh của PlaylistTrack.
    /// Bắt buộc xóa qua Root để đảm bảo tính toàn vẹn của Aggregate.
    /// </summary>
    /// <param name="trackId">Mã định danh của PlaylistTrack cần xóa.</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát với trackId tương ứng trong Playlist.</exception>
    public void RemoveTrack(string trackId)
    {
        PlaylistTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Playlist này.");

        _tracks.Remove(track);
    }

    /// <summary>
    /// Cập nhật thứ tự sắp xếp của một bài hát trong Playlist.
    /// Bắt buộc thực hiện qua Root để đảm bảo tính toàn vẹn của Aggregate.
    /// </summary>
    /// <param name="trackId">Mã định danh của PlaylistTrack cần cập nhật thứ tự.</param>
    /// <param name="newTrackOrder">Giá trị thứ tự mới (từ 1 đến 100).</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát với trackId tương ứng trong Playlist.</exception>
    public void UpdateTrackOrder(string trackId, int newTrackOrder)
    {
        PlaylistTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Playlist này.");

        track.UpdateTrackOrder(newTrackOrder);
    }
}