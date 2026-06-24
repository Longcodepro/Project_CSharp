namespace TuneVault.Application.Features.Playlist.DTOs;

/// <summary>
/// DTO request dùng để tạo playlist mới.
/// </summary>
/// <param name="Title">Tiêu đề playlist.</param>
/// <param name="Description">Mô tả playlist (tùy chọn).</param>
/// <param name="IsPublic">Playlist có công khai hay không.</param>
/// <param name="CoverImgUrl">URL ảnh bìa playlist.</param>
/// <param name="ContentType">Kiểu nội dung chung của playlist: Audio, Song hoặc Video.</param>
/// <param name="ReleaseDate">Ngày phát hành playlist.</param>
public sealed record CreatePlaylistRequestDto(
    string Title,
    string? Description,
    bool IsPublic,
    string? CoverImgUrl,
    string? ContentType,
    DateTime? ReleaseDate);
