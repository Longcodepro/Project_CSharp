namespace TuneVault.Application.DTOs.Search;

/// <summary>
/// Kết quả tìm kiếm kèm thông tin phân trang.
/// </summary>
public sealed record SearchResponseDto(
    SearchResultDto Data,
    int Page,
    int PageSize,
    int TotalMedia,
    int TotalPages);
