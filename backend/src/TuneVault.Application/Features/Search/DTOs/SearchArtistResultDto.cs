namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// Thông tin người dùng trong kết quả tìm kiếm.
/// </summary>
public sealed record SearchArtistResultDto(
    string Id,
    string UserName,
    string DisplayName,
    string? AvatarUrl,
    int TotalFollowers);
