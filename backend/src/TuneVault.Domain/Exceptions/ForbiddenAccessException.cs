namespace TuneVault.Domain.Exceptions;

/// <summary>
/// Exception ném ra khi người dùng không có quyền truy cập tài nguyên.
/// Ánh xạ tới HTTP 403 Forbidden.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}