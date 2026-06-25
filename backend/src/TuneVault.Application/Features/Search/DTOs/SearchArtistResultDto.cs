namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// DTO - SEARCH ARTIST RESULT (Application Layer)
/// =============================================
/// Mục đích: Đại diện cho một nghệ sĩ trong kết quả tìm kiếm.
/// 
/// Sử dụng:
/// - SearchRepository.SearchArtistsAsync() -> dynamic -> SearchArtistResultDto
/// - Được trả về trong SearchResultDto.Artists[]
/// 
/// Tính chất: Record (immutable, value-based equality)
/// Properties:
///   - Id: Mã định danh user/nghệ sĩ
///   - UserName: Tên đăng nhập (3-32 chars)
///   - DisplayName: Tên hiển thị công khai
///   - AvatarUrl: URL ảnh đại diện
///   - TotalFollowers: Số lượng followers
/// </summary>

public sealed record SearchArtistResultDto(
    string Id,
    string UserName,
    string DisplayName,
    string? AvatarUrl,
    int TotalFollowers);
