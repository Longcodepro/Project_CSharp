namespace TuneVault.Application.Common.Responses;

public record ApiResponse(bool Success, string? Message = null)
{
    public static ApiResponse Ok(string? message = null) => new(true, message);
    public static ApiResponse Fail(string message) => new(false, message);
}
