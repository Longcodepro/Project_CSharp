namespace TuneVault.Application.DTOs.Playlist;

/// <summary>
/// DTO request dùng để tạo playlist mới.
/// </summary>
/// <param name="Title">Tiêu đề playlist.</param>
/// <param name="IsPublic">Playlist có công khai hay không.</param>
/// <param name="CoverImgUrl">URL ảnh bìa playlist.</param>
public sealed record CreatePlaylistRequestDto(string Title, bool IsPublic, string? CoverImgUrl);
