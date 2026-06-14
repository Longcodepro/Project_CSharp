namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// DTO - SEARCH PLAYLIST RESULT (Application Layer)
/// ===============================================
/// Mục đích: Đại diện cho một playlist công khai trong kết quả tìm kiếm.
/// 
/// Sử dụng:
/// - SearchRepository.SearchPlaylistsAsync() -> dynamic -> SearchPlaylistResultDto
/// - Được trả về trong SearchResultDto.Playlists[]
/// 
/// Tính chất: Record (immutable, value-based equality)
/// Properties:
///   - Id: Mã định danh playlist
///   - Title: Tên playlist
///   - CoverImageUrl: URL ảnh bìa playlist
///   - OwnerName: Tên chủ sở hữu playlist
///   - TrackCount: Số lượng tracks
///   - CreatedAt: Thời điểm tạo
/// </summary>

public sealed record SearchPlaylistResultDto(
    string Id,
    string Title,
    string? CoverImageUrl,
    string OwnerName,
    int TrackCount,
    DateTime CreatedAt);
