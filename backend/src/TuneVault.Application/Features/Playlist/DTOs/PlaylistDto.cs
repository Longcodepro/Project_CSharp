namespace TuneVault.Application.Features.Playlist.DTOs;

/// <summary>
/// DTO - PLAYLIST RESPONSE (Application Layer)
/// ==========================================
/// Mục đích: Đại diện cho playlist trả về cho client.
/// 
/// Sử dụng:
/// - PlaylistController.GetByUserId() -> PlaylistDto[]
/// - PlaylistController.Create() -> PlaylistDto
/// 
/// Properties:
///   - Id: Mã playlist
///   - OwnerId: Mã user sở hữu playlist
///   - Title: Tiêu đề playlist
///   - Description: Mô tả playlist
///   - CoverImgUrl: URL ảnh bìa
///   - IsPublic: Trạng thái công khai
///   - ContentType: Kiểu nội dung chung của playlist
///   - ReleaseDate: Ngày phát hành playlist
///   - CreatedAt: Thời điểm tạo
///   - Tracks: Danh sách track trong playlist
/// </summary>
public sealed record PlaylistDto(
    string Id,
    string OwnerId,
    string Title,
    string? Description,
    string? CoverImgUrl,
    bool IsPublic,
    string? ContentType,
    DateTime? ReleaseDate,
    DateTime CreatedAt,
    IReadOnlyCollection<PlaylistTrackDto> Tracks);

/// <summary>
/// DTO đại diện cho một bài hát đã được thêm vào playlist.
/// </summary>
/// <param name="MediaItemId">Mã media item trong playlist.</param>
/// <param name="TrackOrder">Thứ tự phát trong playlist.</param>
/// <param name="AddedAt">Thời điểm bài hát được thêm vào playlist.</param>
public sealed record PlaylistTrackDto(
    string MediaItemId,
    int TrackOrder,
    DateTime AddedAt);
