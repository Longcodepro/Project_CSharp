using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một mục phương tiện (nhạc, audio, video...) trong hệ thống TuneVault.
/// Thực thể này độc lập quản lý trạng thái, số lượng tương tác và tính toàn vẹn dữ liệu.
/// Mỗi media thuộc về đúng một người tạo thông qua <see cref="OwnerId"/>.
/// </summary>
public class MediaItem
{
    // --- Constants ---
    private const int MaxTitleLength = 30;
    private const int MaxDescriptionLength = 500;

    // --- Properties ---

    /// <summary>
    /// Mã định danh nghiệp vụ của phương tiện (VD: I001).
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh nghiệp vụ của người sở hữu — là ca sĩ chính (VD: U001).
    /// </summary>
    public string OwnerId { get; private set; } = string.Empty;

    /// <summary>
    /// Tiêu đề của phương tiện. Tối đa 30 ký tự.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết của phương tiện. Tối đa 500 ký tự.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Loại phương tiện (Audio, Video, Song).
    /// </summary>
    public MediaType Type { get; private set; }

    /// <summary>
    /// Đường dẫn file phương tiện (Value Object đảm bảo URL hợp lệ).
    /// </summary>
    public MediaUrl Url { get; private set; } = new MediaUrl("https://placeholder.tunevault.com");

    /// <summary>
    /// Đường dẫn ảnh bìa (thumbnail/cover art).
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Đường dẫn hình ảnh Canvas (hiệu ứng nền động khi phát nhạc).
    /// </summary>
    public string? CanvasUrl { get; private set; }

    /// <summary>
    /// Thể loại âm nhạc (VD: Pop, Rock, Jazz).
    /// </summary>
    public string? Genre { get; private set; }

    /// <summary>
    /// Thời lượng chính thức của phương tiện.
    /// </summary>
    public MediaDuration Duration { get; private set; } = new MediaDuration(0, 0);

    /// <summary>
    /// Trạng thái hiển thị công khai của phương tiện.
    /// </summary>
    public bool IsPublic { get; private set; } = true;

    /// <summary>
    /// Trạng thái hoạt động của phương tiện (Soft Delete — false = đã xóa).
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Tổng số lượt yêu thích (phi chuẩn hóa để tối ưu hiệu năng hiển thị).
    /// </summary>
    public int FavoriteCount { get; private set; }

    /// <summary>
    /// Số lượt xem/nghe phương tiện.
    /// </summary>
    public int ViewCount { get; private set; }

    /// <summary>
    /// Thời điểm phương tiện được tải lên hệ thống (UTC).
    /// </summary>
    public DateTime UploadedAt { get; private set; }

    /// <summary>
    /// Ngày phát hành chính thức (nullable — có thể phát hành ngay hoặc đặt lịch).
    /// </summary>
    public DateTime? ReleaseDate { get; private set; }

    // --- Constructors ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper khi map dữ liệu từ DB.
    /// </summary>
    private MediaItem() { }

    public static MediaItem Hydrate(
        string id,
        string ownerId,
        string title,
        string? description,
        MediaType type,
        string mediaUrl,
        string? coverImageUrl,
        string? canvasUrl,
        string? genre,
        int durationMinutes,
        int durationSeconds,
        bool isPublic,
        bool isActive,
        int favoriteCount,
        int viewCount,
        DateTime uploadedAt,
        DateTime? releaseDate)
    {
        if (durationMinutes < 0)
            durationMinutes = 0;

        if (durationSeconds < 0)
            durationSeconds = 0;

        if (durationSeconds >= 60)
        {
            durationMinutes += durationSeconds / 60;
            durationSeconds %= 60;
        }

        return new MediaItem
        {
            Id = id.Trim(),
            OwnerId = ownerId.Trim(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Type = type,
            Url = new MediaUrl(mediaUrl),
            CoverImageUrl = coverImageUrl?.Trim(),
            CanvasUrl = canvasUrl?.Trim(),
            Genre = genre?.Trim(),
            Duration = new MediaDuration(durationMinutes, durationSeconds),
            IsPublic = isPublic,
            IsActive = isActive,
            FavoriteCount = favoriteCount,
            ViewCount = viewCount,
            UploadedAt = uploadedAt,
            ReleaseDate = releaseDate
        };
    }

    /// <summary>
    /// Khởi tạo một <see cref="MediaItem"/> mới với các thông tin bắt buộc.
    /// </summary>
    /// <param name="id">Mã định danh nghiệp vụ.</param>
    /// <param name="ownerId">Mã định danh ca sĩ chính sở hữu bài hát.</param>
    /// <param name="title">Tiêu đề phương tiện (tối đa 30 ký tự).</param>
    /// <param name="type">Loại phương tiện.</param>
    /// <param name="mediaUrl">Value Object chứa URL file phương tiện.</param>
    public MediaItem(string id, string ownerId, string title, MediaType type, MediaUrl mediaUrl)
    {
        ValidateMediaId(id);
        ValidateOwnerId(ownerId);
        ValidateTitle(title);

        Id = id.Trim();
        OwnerId = ownerId.Trim();
        Title = title.Trim();
        Type = type;
        Url = mediaUrl;
        UploadedAt = DateTime.UtcNow;
        IsActive = true;

        FavoriteCount = 0;
        ViewCount = 0;
        Duration = new MediaDuration(0, 0);
    }

    // --- Business Methods ---

    /// <summary>
    /// Cập nhật các thông tin chi tiết và mô tả của phương tiện.
    /// </summary>
    /// <param name="title">Tiêu đề mới (tối đa 30 ký tự).</param>
    /// <param name="description">Mô tả mới (tối đa 500 ký tự, nullable).</param>
    /// <param name="genre">Thể loại mới (nullable).</param>
    /// <exception cref="DomainException">Ném ra nếu bài hát đã bị xóa (IsActive = false).</exception>
    public void UpdateDetails(string title, string? description, string? genre)
    {
        EnsureActive();
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title.Trim();
        Description = description?.Trim();
        Genre = genre?.Trim();
    }

    /// <summary>
    /// Thiết lập đường dẫn ảnh bìa sau khi xác thực tham chiếu lưu trữ.
    /// </summary>
    /// <param name="coverUrl">URL, storage key hoặc path nội bộ của ảnh bìa.</param>
    /// <exception cref="DomainException">Ném ra nếu URL không hợp lệ.</exception>
    public void SetCoverImage(string coverUrl)
    {
        ValidateUrl(coverUrl, nameof(CoverImageUrl));
        CoverImageUrl = coverUrl.Trim();
    }

    /// <summary>
    /// Thiết lập đường dẫn Canvas sau khi xác thực tham chiếu lưu trữ.
    /// </summary>
    /// <param name="canvasUrl">URL, storage key hoặc path nội bộ của canvas.</param>
    /// <exception cref="DomainException">Ném ra nếu URL không hợp lệ.</exception>
    public void SetCanvas(string canvasUrl)
    {
        ValidateUrl(canvasUrl, nameof(CanvasUrl));
        CanvasUrl = canvasUrl.Trim();
    }

    /// <summary>
    /// Thiết lập lại URL file media gốc sau khi thay file upload.
    /// </summary>
    /// <param name="mediaUrl">URL file media mới.</param>
    public void SetMediaUrl(MediaUrl mediaUrl)
    {
        Url = mediaUrl ?? throw new DomainException("MediaUrl không được để trống.");
    }

    /// <summary>
    /// Cập nhật trạng thái công khai của media.
    /// </summary>
    /// <param name="isPublic">True nếu media được hiển thị công khai.</param>
    public void SetVisibility(bool isPublic)
    {
        EnsureActive();
        IsPublic = isPublic;
    }

    /// <summary>
    /// Thiết lập thời lượng chính cho phương tiện sau khi xử lý file.
    /// </summary>
    /// <param name="minutes">Số phút.</param>
    /// <param name="seconds">Số giây.</param>
    public void SetDuration(int minutes, int seconds)
        => Duration = new MediaDuration(minutes, seconds);

    /// <summary>
    /// Tăng tổng số lượt yêu thích lên 1 khi có User bấm Thích.
    /// </summary>
    public void IncrementFavoriteCount()
    {
        int targetCount = FavoriteCount + 1;
        ValidateFavoriteCount(targetCount);
        FavoriteCount = targetCount;
    }

    /// <summary>
    /// Giảm tổng số lượt yêu thích đi 1 khi có User hủy Thích.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu số lượt yêu thích sẽ xuống dưới 0.</exception>
    public void DecrementFavoriteCount()
    {
        int targetCount = FavoriteCount - 1;
        ValidateFavoriteCount(targetCount);
        FavoriteCount = targetCount;
    }

    /// <summary>
    /// Tăng lượt xem cho phương tiện lên 1.
    /// </summary>
    public void IncrementViewCount() => ViewCount++;

    /// <summary>
    /// Thực hiện Soft Delete: chuyển <see cref="IsActive"/> về <c>false</c>.
    /// Chỉ Owner (ca sĩ chính) mới được gọi thao tác này — logic kiểm tra quyền ở Handler.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu bài hát đã bị xóa trước đó.</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Bài hát này đã bị xóa trước đó.");

        IsActive = false;
    }

    // --- Private Guard ---

    /// <summary>
    /// Kiểm tra bài hát có đang hoạt động trước khi cho phép thay đổi.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu bài hát đã bị vô hiệu hóa.</exception>
    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("Không thể chỉnh sửa bài hát đã bị xóa.");
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>Kiểm tra tính hợp lệ của mã định danh bài hát.</summary>
    private static void ValidateMediaId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Mã bài hát không được để trống.");
    }

    /// <summary>Kiểm tra tính hợp lệ của mã định danh người sở hữu.</summary>
    private static void ValidateOwnerId(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new DomainException("Mã người sở hữu không được để trống.");
    }

    /// <summary>Kiểm tra tính hợp lệ của tiêu đề (không rỗng, không quá 30 ký tự).</summary>
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề không được để trống.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề không được vượt quá {MaxTitleLength} ký tự.");
    }

    /// <summary>Kiểm tra tính hợp lệ của mô tả (không quá 500 ký tự).</summary>
    private static void ValidateDescription(string? description)
    {
        if (description?.Length > MaxDescriptionLength)
            throw new DomainException($"Mô tả không được vượt quá {MaxDescriptionLength} ký tự.");
    }

    /// <summary>Kiểm tra tham chiếu lưu trữ file không rỗng.</summary>
    private static void ValidateUrl(string url, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException($"{fieldName} không được để trống.");
    }

    /// <summary>Kiểm tra biến đếm lượt yêu thích không được âm.</summary>
    private static void ValidateFavoriteCount(int count)
    {
        if (count < 0)
            throw new DomainException("Số lượt yêu thích không được phép nhỏ hơn 0.");
    }
}
