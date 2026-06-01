namespace TuneVault.Application.Common.Responses;

public record ApiResponse<T>(bool Success, T? Data = default, string[]? Errors = null)
{
    public static ApiResponse<T> Ok(T data) => new(true, data);

    public static ApiResponse<T> Fail(params string[] errors) => new(false, default, errors);
}
