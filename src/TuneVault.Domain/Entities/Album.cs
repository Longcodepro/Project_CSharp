using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một Album trong hệ thống TuneVault.
/// Quản lý thông tin, trạng thái và các quy tắc nghiệp vụ của Album.
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

    /// <summary>Mã định danh nghiệp vụ (Primary Key) của Album.</summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>Mã định danh của nghệ sĩ sở hữu Album.</summary>
    public string ArtistId { get; private set; } = string.Empty;

    /// <summary>Tiêu đề của Album (tối đa 24 ký tự).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Mô tả chi tiết về Album (nullable, tối đa 500 ký tự).</summary>
    public string? Description { get; private set; }

    /// <summary>Đường dẫn URL ảnh bìa Album (nullable).</summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động của Album (Soft Delete — <c>false</c> = đã xóa/ẩn).
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Thời điểm khởi tạo Album trong hệ thống (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Danh sách bài hát trong Album (chỉ đọc để đảm bảo tính đóng gói).</summary>
    public IReadOnlyCollection<AlbumTrack> Tracks => _tracks.AsReadOnly();

    // --- Constructor ---

    /// <summary>Constructor rỗng bắt buộc cho Dapper khi map dữ liệu từ DB.</summary>
    private Album() { }

    /// <summary>
    /// Khởi tạo một Album mới với đầy đủ thông tin bắt buộc.
    /// </summary>
    /// <param name="id">Mã định danh Album (tối đa 4 ký tự).</param>
    /// <param name="artistId">Mã định danh nghệ sĩ sở hữu.</param>
    /// <param name="title">Tiêu đề Album (tối đa 24 ký tự).</param>
    /// <param name="description">Mô tả Album (nullable).</param>
    /// <param name="coverImageUrl">Đường dẫn ảnh bìa (nullable).</param>
    public Album(string id, string artistId, string title,
        string? description = null, string? coverImageUrl = null)
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
        IsActive = true;
        CreatedAt = now;
    }

    // --- Business Methods ---

    /// <summary>Cập nhật tiêu đề Album.</summary>
    /// <param name="title">Tiêu đề mới.</param>
    public void Rename(string title)
    {
        EnsureActive();
        ValidateTitle(title);
        Title = title.Trim();
    }

    /// <summary>Cập nhật mô tả Album.</summary>
    /// <param name="description">Nội dung mô tả mới (nullable).</param>
    public void UpdateDescription(string? description)
    {
        EnsureActive();
        ValidateDescription(description);
        Description = description?.Trim();
    }

    /// <summary>Cập nhật đường dẫn ảnh bìa Album.</summary>
    /// <param name="coverImageUrl">URL ảnh bìa mới (nullable).</param>
    public void UpdateCoverImage(string? coverImageUrl)
    {
        EnsureActive();
        ValidateCoverImageUrl(coverImageUrl);
        CoverImageUrl = coverImageUrl?.Trim();
    }

    /// <summary>
    /// Thêm một bài hát (AlbumTrack) vào Album.
    /// </summary>
    /// <param name="track">Thực thể AlbumTrack cần thêm.</param>
    /// <exception cref="DomainException">Ném ra khi track là null hoặc không thuộc Album này.</exception>
    public void AddTrack(AlbumTrack track)
    {
        EnsureActive();
        if (track is null)
            throw new DomainException("Bài hát thêm vào Album không được là null.");

        if (track.AlbumId != Id)
            throw new DomainException("Bài hát không thuộc về Album này.");

        _tracks.Add(track);
    }

    /// <summary>
    /// Xóa một bài hát khỏi Album theo mã định danh AlbumTrack.
    /// </summary>
    /// <param name="trackId">Mã định danh AlbumTrack cần xóa.</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát.</exception>
    public void RemoveTrack(string trackId)
    {
        EnsureActive();
        AlbumTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Album này.");

        _tracks.Remove(track);
    }

    /// <summary>
    /// Cập nhật thứ tự sắp xếp bài hát trong Album.
    /// </summary>
    /// <param name="trackId">Mã định danh AlbumTrack cần cập nhật.</param>
    /// <param name="newTrackOrder">Thứ tự mới.</param>
    /// <exception cref="DomainException">Ném ra khi không tìm thấy bài hát.</exception>
    public void UpdateTrackOrder(string trackId, int newTrackOrder)
    {
        AlbumTrack? track = _tracks.Find(t => t.Id == trackId);
        if (track is null)
            throw new DomainException($"Không tìm thấy bài hát với Id '{trackId}' trong Album này.");

        track.UpdateTrackOrder(newTrackOrder);
    }

    /// <summary>
    /// Thực hiện Soft Delete — vô hiệu hóa Album (không xóa khỏi DB).
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu Album đã bị xóa trước đó.</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Album này đã bị xóa trước đó.");

        IsActive = false;
    }

    // --- Private Guard ---

    /// <summary>Kiểm tra Album còn active trước khi thực hiện thao tác.</summary>
    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("Không thể thực hiện thao tác trên Album đã bị xóa.");
    }

    // --- Validation Methods ---

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id Album không được để trống.");

        if (id.Trim().Length > MaxIdLength)
            throw new DomainException($"Id Album không được vượt quá {MaxIdLength} ký tự.");
    }

    private static void ValidateArtistId(string artistId)
    {
        if (string.IsNullOrWhiteSpace(artistId))
            throw new DomainException("ArtistId không được để trống.");

        if (artistId.Trim().Length > MaxIdLength)
            throw new DomainException($"ArtistId không được vượt quá {MaxIdLength} ký tự.");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề Album không được để trống.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề Album không được vượt quá {MaxTitleLength} ký tự.");
    }

    private static void ValidateDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > MaxDescriptionLength)
            throw new DomainException($"Mô tả Album không được vượt quá {MaxDescriptionLength} ký tự.");
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
            throw new DomainException("Đường dẫn ảnh bìa không hợp lệ (phải là HTTP/HTTPS).");
        }
    }

    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo Album không hợp lệ.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo Album không được vượt quá thời gian hiện tại.");
    }
}
