namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// Nhóm kết quả tìm kiếm theo media, user và playlist.
/// </summary>
public sealed record SearchResultDto(
    IReadOnlyCollection<SearchMediaResultDto>? Media,
    IReadOnlyCollection<SearchArtistResultDto>? Artists,
    IReadOnlyCollection<SearchPlaylistResultDto>? Playlists,
    IReadOnlyCollection<SearchMediaResultDto>? TrendingMedia,
    int TotalCount);
