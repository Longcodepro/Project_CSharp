namespace TuneVault.Domain.ValueObjects;

/// <summary>
/// Value Object đại diện cho đường dẫn tệp tin media.
/// Đảm bảo URL luôn hợp lệ và không rỗng.
/// </summary>
public record MediaUrl
{
    public string Value { get; }

    public MediaUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("URL không được để trống.");
        
        Value = value.Trim();
    }

    public static implicit operator string(MediaUrl url) => url.Value;
}