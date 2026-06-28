namespace TuneVault.Application.Features.Playlist.DTOs;

/// <summary>
/// Thông tin playlist trả về cho client.
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
