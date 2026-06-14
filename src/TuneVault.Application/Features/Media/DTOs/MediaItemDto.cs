namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// DTO trả về thông tin đầy đủ của một bài hát/media (không bao gồm file stream).
/// Dùng trong: GetMediaById, UploadMedia (response), UpdateMedia (response).
/// </summary>
/// <param name="Id">Mã định danh nội bộ (VD: I001).</param>
/// <param name="OwnerId">Mã định danh ca sĩ chính sở hữu bài hát.</param>
/// <param name="Title">Tiêu đề bài hát.</param>
/// <param name="Description">Mô tả bài hát (nullable).</param>
/// <param name="Genre">Thể loại âm nhạc (nullable).</param>
/// <param name="Type">Loại media: Audio, Video, Podcast, Song.</param>
/// <param name="AudioUrl">Đường dẫn file audio (nếu có).</param>
/// <param name="VideoUrl">Đường dẫn file video (nếu có).</param>
/// <param name="CoverImageUrl">Đường dẫn ảnh bìa (nullable).</param>
/// <param name="CanvasUrl">Đường dẫn canvas động (nullable).</param>
/// <param name="DurationSeconds">Tổng thời lượng theo giây.</param>
/// <param name="AccessLevel">Cấp độ truy cập: Normal hoặc Premium.</param>
/// <param name="IsPublic">Trạng thái hiển thị công khai.</param>
/// <param name="IsActive">Trạng thái hoạt động (false = đã xóa).</param>
/// <param name="FavoriteCount">Số lượt yêu thích.</param>
/// <param name="ViewCount">Số lượt xem/nghe.</param>
/// <param name="UploadedAt">Thời điểm tải lên (UTC).</param>
/// <param name="ReleaseDate">Ngày phát hành chính thức (nullable).</param>
/// <param name="Artists">Danh sách nghệ sĩ tham gia (ca sĩ chính + ca sĩ phụ).</param>
public sealed record MediaItemDto(
    string Id,
    string OwnerId,
    string Title,
    string? Description,
    string? Genre,
    string Type,
    string? AudioUrl,
    string? VideoUrl,
    string? CoverImageUrl,
    string? CanvasUrl,
    int DurationSeconds,
    string AccessLevel,
    bool IsPublic,
    bool IsActive,
    int FavoriteCount,
    int ViewCount,
    DateTime UploadedAt,
    DateTime? ReleaseDate,
    IEnumerable<MediaArtistDto> Artists
);

/// <summary>
/// DTO đại diện cho một nghệ sĩ tham gia bài hát.
/// </summary>
/// <param name="ArtistId">Mã định danh nghệ sĩ.</param>
/// <param name="Role">Vai trò: MainArtist hoặc FeaturedArtist.</param>
public sealed record MediaArtistDto(
    string ArtistId,
    string Role
);