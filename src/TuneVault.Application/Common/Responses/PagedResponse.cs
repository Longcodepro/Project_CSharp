namespace TuneVault.Application.Common.Responses;

public record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Message = null) : ApiResponse(true, Message);
