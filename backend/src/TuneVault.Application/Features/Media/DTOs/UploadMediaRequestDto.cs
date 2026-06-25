namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// DTO nhận dữ liệu từ client khi upload một bài hát mới.
/// Ca sĩ chính (OwnerId) được truyền riêng qua route/param để phân biệt rõ quyền sở hữu.
/// </summary>
/// <param name="OwnerId">Mã định danh ca sĩ chính — người sở hữu và chịu trách nhiệm bài hát.</param>
/// <param name="Title">Tiêu đề bài hát (tối đa 30 ký tự, bắt buộc).</param>
/// <param name="Description">Mô tả bài hát (nullable, tối đa 500 ký tự).</param>
/// <param name="Genre">Thể loại âm nhạc (nullable).</param>
/// <param name="Type">Loại media: Audio, Video, Podcast, Song.</param>
/// <param name="AudioUrl">Đường dẫn file audio (nullable — dùng khi Type là Audio/Song/Podcast).</param>
/// <param name="VideoUrl">Đường dẫn file video (nullable — dùng khi Type là Video).</param>
/// <param name="CoverImageUrl">Đường dẫn ảnh bìa (nullable).</param>
/// <param name="CanvasUrl">Đường dẫn canvas động (nullable — chỉ dùng cho Audio/Song).</param>
/// <param name="AccessLevel">Cấp độ truy cập: Normal (0) hoặc Premium (1).</param>
/// <param name="IsPublic">Trạng thái công khai (mặc định <c>true</c>).</param>
/// <param name="ReleaseDate">Ngày phát hành chính thức (nullable — null = phát hành ngay).</param>
/// <param name="FeaturedArtistIds">
/// Danh sách mã định danh ca sĩ phụ (FeaturedArtist).
/// Mỗi ca sĩ phụ không có quyền sở hữu bài hát — chỉ được credit.
/// Có thể rỗng nếu bài hát chỉ có một ca sĩ.
/// </param>
public sealed record UploadMediaRequestDto(
    string OwnerId,
    string Title,
    string? Description,
    string? Genre,
    string Type,
    string? AudioUrl,
    string? VideoUrl,
    string? CoverImageUrl,
    string? CanvasUrl,
    int AccessLevel,
    bool IsPublic,
    DateTime? ReleaseDate,
    IEnumerable<string> FeaturedArtistIds
);