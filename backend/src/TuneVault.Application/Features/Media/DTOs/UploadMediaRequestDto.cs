namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// DTO nhận dữ liệu từ client khi upload một bài hát mới.
/// Ca sĩ chính (OwnerId) được truyền riêng qua route/param để phân biệt rõ quyền sở hữu.
/// </summary>
/// <param name="OwnerId">Mã định danh ca sĩ chính — người sở hữu và chịu trách nhiệm bài hát.</param>
/// <param name="Title">Tiêu đề bài hát (tối đa 30 ký tự, bắt buộc).</param>
/// <param name="Description">Mô tả bài hát (nullable, tối đa 500 ký tự).</param>
/// <param name="Genre">Thể loại âm nhạc (nullable).</param>
/// <param name="Type">Loại media: Audio, Video, Song.</param>
/// <param name="AudioUrl">Đường dẫn file audio (nullable — dùng khi Type là Audio/Song).</param>
/// <param name="VideoUrl">Đường dẫn file video (nullable — dùng khi Type là Video).</param>
/// <param name="CoverImageUrl">Đường dẫn ảnh bìa (nullable).</param>
/// <param name="CanvasUrl">Đường dẫn canvas động (nullable — chỉ dùng cho Audio/Song).</param>
/// <param name="DurationSeconds">Thời lượng media theo giây.</param>
/// <param name="IsPublic">Trạng thái công khai (mặc định <c>true</c>).</param>
/// <param name="ReleaseDate">Ngày phát hành chính thức (nullable — null = phát hành ngay).</param>
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
    int? DurationSeconds,
    bool IsPublic,
    DateTime? ReleaseDate
);
