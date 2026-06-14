namespace TuneVault.Application.DTOs.Playlist;

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
///   - CoverImgUrl: URL ảnh bìa
///   - IsPublic: Trạng thái công khai
///   - CreatedAt: Thời điểm tạo
/// </summary>
public sealed record PlaylistDto(
    string Id,
    string OwnerId,
    string Title,
    string? CoverImgUrl,
    bool IsPublic,
    DateTime CreatedAt);
