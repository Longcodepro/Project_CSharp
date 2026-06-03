using System;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một mục phương tiện (nhạc, podcast, v.v.) trong hệ thống TuneVault.
/// Đây là một thực thể độc lập quản lý trạng thái, số lượng tương tác và tính toàn vẹn của dữ liệu phương tiện.
/// </summary>
public class MediaItem
{
    // --- Constants ---
    private const int MaxTitleLength = 30;
    private const int MaxDescriptionLength = 500;

    // --- Properties ---

    /// <summary>
    /// Mã định danh nghiệp vụ của phương tiện (ví dụ: I001).
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh nghiệp vụ của người sở hữu (ví dụ: U001).
    /// </summary>
    public string OwnerId { get; private set; }

    /// <summary>
    /// Tiêu đề của phương tiện.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết của phương tiện.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Loại phương tiện (Nhạc, Podcast, v.v.).
    /// </summary>
    public MediaType Type { get; private set; }

    /// <summary>
    /// Đường dẫn file phương tiện (Value Object).
    /// </summary>
    public MediaUrl Url { get; private set; }

    /// <summary>
    /// Đường dẫn ảnh bìa.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Đường dẫn hình ảnh Canvas.
    /// </summary>
    public string? CanvasUrl { get; private set; }

    /// <summary>
    /// Thể loại phương tiện.
    /// </summary>
    public string? Genre { get; private set; }

    /// <summary>
    /// Thời lượng chính thức của phương tiện.
    /// </summary>
    public MediaDuration Duration { get; private set; }

    /// <summary>
    /// Thời lượng đoạn trailer (nếu có).
    /// </summary>
    public MediaDuration DurationTrailer { get; private set; }

    /// <summary>
    /// Cấp độ truy cập (Public, Private, v.v.).
    /// </summary>
    public AccessLevel AccessLevel { get; private set; }

    /// <summary>
    /// Trạng thái công khai của phương tiện.
    /// </summary>
    public bool IsPublic { get; private set; } = true;

    /// <summary>
    /// Tổng số lượt bày tỏ cảm xúc / yêu thích của bài hát (Dữ liệu phi chuẩn hóa phục vụ tối ưu hóa hiệu năng hiển thị).
    /// </summary>
    public int FavoriteCount { get; private set; }

    /// <summary>
    /// Số lượt xem phương tiện.
    /// </summary>
    public int ViewCount { get; private set; }

    /// <summary>
    /// Thời điểm phương tiện được tải lên hệ thống.
    /// </summary>
    public DateTime UploadedAt { get; private set; }

    /// <summary>
    /// Ngày phát hành chính thức của phương tiện.
    /// </summary>
    public DateTime? ReleaseDate { get; private set; }

    // --- Constructors ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM khi map dữ liệu từ DB.
    /// </summary>
    private MediaItem() { }

    /// <summary>
    /// Khởi tạo một đối tượng MediaItem mới với các định danh và thông tin bắt buộc.
    /// </summary>
    /// <param name="id">Mã định danh nghiệp vụ cho phương tiện.</param>
    /// <param name="ownerId">Mã định danh nghiệp vụ cho người sở hữu.</param>
    /// <param name="title">Tiêu đề phương tiện.</param>
    /// <param name="type">Loại phương tiện.</param>
    /// <param name="mediaUrl">Value Object chứa URL file.</param>
    /// <param name="accessLevel">Cấp độ truy cập.</param>
    public MediaItem(string id, string ownerId, string title, MediaType type, MediaUrl mediaUrl, AccessLevel accessLevel)
    {
        ValidateMediaId(id);
        ValidateOwnerId(ownerId);
        ValidateTitle(title);

        Id = id.Trim();
        OwnerId = ownerId.Trim();
        Title = title.Trim();
        Type = type;
        Url = mediaUrl;
        AccessLevel = accessLevel;
        UploadedAt = DateTime.UtcNow;
        
        FavoriteCount = 0; // Mới tạo mặc định chưa có lượt thích nào
        ViewCount = 0;
        Duration = new MediaDuration(0, 0);
        DurationTrailer = new MediaDuration(0, 0);
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật các thông tin chi tiết và mô tả của phương tiện.
    /// </summary>
    /// <param name="title">Tiêu đề mới.</param>
    /// <param name="description">Mô tả mới.</param>
    /// <param name="genre">Thể loại mới.</param>
    public void UpdateDetails(string title, string? description, string? genre)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title.Trim();
        Description = description?.Trim();
        Genre = genre?.Trim();
    }

    /// <summary>
    /// Thiết lập đường dẫn ảnh bìa sau khi xác thực URL.
    /// </summary>
    /// <param name="coverUrl">Đường dẫn ảnh bìa.</param>
    public void SetCoverImage(string coverUrl)
    {
        ValidateUrl(coverUrl, nameof(CoverImageUrl));
        CoverImageUrl = coverUrl.Trim();
    }

    /// <summary>
    /// Thiết lập đường dẫn Canvas sau khi xác thực URL.
    /// </summary>
    /// <param name="canvasUrl">Đường dẫn hình ảnh Canvas.</param>
    public void SetCanvas(string canvasUrl)
    {
        ValidateUrl(canvasUrl, nameof(CanvasUrl));
        CanvasUrl = canvasUrl.Trim();
    }

    /// <summary>
    /// Cập nhật chính sách truy cập và cấu hình thời lượng trailer.
    /// </summary>
    /// <param name="accessLevel">Cấp độ truy cập mới.</param>
    /// <param name="trailerMinutes">Số phút trailer.</param>
    /// <param name="trailerSeconds">Số giây trailer.</param>
    public void UpdateAccessPolicy(AccessLevel accessLevel, int trailerMinutes, int trailerSeconds)
    {
        var trailer = new MediaDuration(trailerMinutes, trailerSeconds);
        
        if (Duration.ToTimeSpan() > TimeSpan.Zero && trailer.ToTimeSpan() > Duration.ToTimeSpan())
            throw new DomainException("Trailer không thể dài hơn thời lượng bài hát gốc.");

        AccessLevel = accessLevel;
        DurationTrailer = trailer;
    }

    /// <summary>
    /// Thiết lập thời lượng chính cho phương tiện sau khi xử lý file.
    /// </summary>
    /// <param name="minutes">Số phút.</param>
    /// <param name="seconds">Số giây.</param>
    public void SetDuration(int minutes, int seconds) 
        => Duration = new MediaDuration(minutes, seconds);

    /// <summary>
    /// Tăng tổng số lượng cảm xúc/yêu thích của bài hát lên 1 đơn vị khi có người dùng bấm Thích.
    /// </summary>
    public void IncrementFavoriteCount()
    {
        int targetCount = FavoriteCount + 1;
        ValidateFavoriteCount(targetCount);
        FavoriteCount = targetCount;
    }

    /// <summary>
    /// Giảm tổng số lượng cảm xúc/yêu thích của bài hát đi 1 đơn vị khi có người dùng hủy bỏ Thích.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu việc giảm làm số lượng yêu thích âm xuống dưới 0.</exception>
    public void DecrementFavoriteCount()
    {
        int targetCount = FavoriteCount - 1;
        ValidateFavoriteCount(targetCount);
        FavoriteCount = targetCount;
    }

    /// <summary>
    /// Tăng lượt xem cho phương tiện.
    /// </summary>
    public void IncrementViewCount() => ViewCount++;

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã định danh bài hát.
    /// </summary>
    private static void ValidateMediaId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Mã bài hát không được để trống.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã định danh người sở hữu.
    /// </summary>
    private static void ValidateOwnerId(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new DomainException("Mã người sở hữu không được để trống.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của tiêu đề (không rỗng, không quá dài).
    /// </summary>
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề không được để trống.");
            
        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề không được vượt quá {MaxTitleLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mô tả (không quá dài).
    /// </summary>
    private static void ValidateDescription(string? description)
    {
        if (description?.Length > MaxDescriptionLength)
            throw new DomainException($"Mô tả không được vượt quá {MaxDescriptionLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của đường dẫn URL (phải bắt đầu bằng http/https).
    /// </summary>
    private static void ValidateUrl(string url, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException($"{fieldName} không được để trống.");

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            throw new DomainException($"{fieldName} phải là một đường dẫn hợp lệ.");
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của biến đếm lượt yêu thích, chặn đứng các hành vi lỗi logic khiến số lượt yêu thích bị âm.
    /// </summary>
    /// <param name="count">Giá trị biến đếm cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi số lượng yêu thích nhỏ hơn 0.</exception>
    private static void ValidateFavoriteCount(int count)
    {
        if (count < 0)
            throw new DomainException("Tổng số lượt cảm xúc/yêu thích của bài hát không được phép nhỏ hơn 0.");
    }
}