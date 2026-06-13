using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một Danh sách phát (Playlist) trong hệ thống TuneVault.
/// Thực thể quản lý thông tin, ảnh bìa, danh sách bài hát và các quy tắc nghiệp vụ.
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

    /// <summary>Mã định danh duy nhất (Primary Key) của Playlist (4–5 ký tự).</summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>Mã định danh của User sở hữu Playlist (4–5 ký tự).</summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>Tiêu đề hiển thị của Playlist (tối đa 100 ký tự).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Mô tả nội dung/chủ đề của Playlist (tối đa 500 ký tự, nullable).</summary>
    public string? Description { get; private set; }

    /// <summary>Đường dẫn URL ảnh bìa của Playlist (nullable).</summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>Trạng thái hiển thị công khai (<c>true</c>) hoặc riêng tư (<c>false</c>).</summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động của Playlist (Soft Delete — <c>false</c> = đã xóa).
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Thời điểm khởi tạo Playlist trong hệ thống (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Danh sách bài hát trong Playlist (chỉ đọc để đảm bảo tính đóng gói).</summary>
    public IReadOnlyCollection<PlaylistTrack> Tracks => _tracks.AsReadOnly();

    // --- Constructor ---

    /// <summary>Constructor rỗng bắt buộc cho Dapper khi map dữ liệu từ DB.</summary>
    private Playlist() { }

    /// <summary>
    /// Khởi tạo một Playlist mới với các thông tin ràng buộc nghiêm ngặt.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất (4–5 ký tự).</param>
    /// <param name="userId">Mã định danh User sở hữu.</param>
    /// <param name="title">Tiêu đề Playlist (tối đa 100 ký tự).</param>
    /// <param name="description">Mô tả (nullable, tối đa 500 ký tự).</param>
    /// <param name="coverImageUrl">URL ảnh bìa (nullable, phải là http/https).</param>
    /// <param name="isPublic">Trạng thái công khai (mặc định <c>false</c>).</param>
    public Playlist(string id, string userId, string title, string? description = null,
        string? coverImageUrl = null, bool isPublic = false)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateTitle(title);
        ValidateDescription(description);
        ValidateCoverImageUrl(coverImageUrl);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        UserId = userId.Trim();
        Title = title.Trim();
        Description = description?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        IsPublic = isPublic;
        IsActive = true;
        CreatedAt = now;
    }

    // --- Business Methods ---

    /// <summary>Đổi tên Playlist.</summary>
    /// <param name="title">Tiêu đề mới.</param>
    public void Rename(string title)
    {
        EnsureActive();
        ValidateTitle(title);
        Title = title.Trim();
    }

    /// <summary>Cập nhật mô tả Playlist.</summary>
    /// <param name="description">Nội dung mô tả mới (nullable).</param>
    public void UpdateDescription(string? description)
    {
        EnsureActive();
        ValidateDescription(description);
        Description = description?.Trim();
    }

    /// <summary>Cập nhật đường dẫn ảnh bìa Playlist.</summary>
    /// <param name="coverImageUrl">URL ảnh bìa mới (nullable).</param>
    public void UpdateCoverImage(string? coverImageUrl)
    {
        EnsureActive();
        ValidateCoverImageUrl(coverImageUrl);
        CoverImageUrl = coverImageUrl?.Trim();
    }

    /// <summary>Thay đổi trạng thái hiển thị công khai/riêng tư.</summary>
    /// <param name="isPublic">Trạng thái mới.</param>
    public void SetPublic(bool isPublic)
    {
        EnsureActive();
        IsPublic = isPublic;
    }

    /// <summary>
    /// Thêm một bài hát (PlaylistTrack) vào danh sách phát.
    /// </summary>
    /// <param name="track">Thực thể bài hát cần thêm.</param>
    /// <exception cref="DomainException">Ném ra khi track là null hoặc Playlist đã bị xóa.</exception>
    public void AddTrack(PlaylistTrack track)
    {
        EnsureActive();
        ValidateTrack(track);
        _tracks.Add(track);
    }

    /// <summary>
    /// Xóa một bài hát khỏi Playlist theo mã định danh PlaylistTrack.
    /// </summary>
    /// <param name="trackId">Mã định danh PlaylistTrack cần xóa.</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát với trackId tương ứng.</exception>
    public void RemoveTrack(string trackId)
    {
        EnsureActive();
        PlaylistTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Playlist này.");

        _tracks.Remove(track);
    }

    /// <summary>
    /// Cập nhật thứ tự sắp xếp bài hát trong Playlist.
    /// </summary>
    /// <param name="trackId">Mã định danh PlaylistTrack cần cập nhật.</param>
    /// <param name="newTrackOrder">Thứ tự mới.</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát.</exception>
    public void UpdateTrackOrder(string trackId, int newTrackOrder)
    {
        PlaylistTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Playlist này.");

        track.UpdateTrackOrder(newTrackOrder);
    }

    /// <summary>
    /// Thực hiện Soft Delete — vô hiệu hóa Playlist.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu Playlist đã bị xóa trước đó.</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Playlist này đã bị xóa trước đó.");

        IsActive = false;
    }

    // --- Private Guard ---

    /// <summary>Kiểm tra Playlist còn active trước khi thực hiện thao tác.</summary>
    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("Không thể thực hiện thao tác trên Playlist đã bị xóa.");
    }

    // --- Validation Methods ---

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id Playlist không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id Playlist phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề Playlist không được để trống.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề Playlist không được vượt quá {MaxTitleLength} ký tự.");
    }

    private static void ValidateDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > MaxDescriptionLength)
            throw new DomainException($"Mô tả Playlist không được vượt quá {MaxDescriptionLength} ký tự.");
    }

    private static void ValidateCoverImageUrl(string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(coverImageUrl)) return;

        string trimmedUrl = coverImageUrl.Trim();
        if (trimmedUrl.Length > MaxUrlLength)
            throw new DomainException($"Đường dẫn ảnh bìa không được vượt quá {MaxUrlLength} ký tự.");

        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out Uri? uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("Đường dẫn ảnh bìa không hợp lệ (phải bắt đầu bằng HTTP hoặc HTTPS).");
        }
    }

    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo Playlist không hợp lệ.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo Playlist không được vượt quá thời gian hiện tại.");
    }

    private static void ValidateTrack(PlaylistTrack track)
    {
        if (track == null)
            throw new DomainException("Bài hát thêm vào Playlist không được là null.");
    }
}
