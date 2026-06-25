using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.ValueObjects;

/// <summary>
/// Đại diện cho tài nguyên media của quảng cáo, bao gồm URL và thời lượng phát.
/// </summary>
public record AdMedia
{
    /// <summary>
    /// Đường dẫn đến tài nguyên quảng cáo.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Thời lượng phát quảng cáo tính bằng giây.
    /// </summary>
    public int DurationInSeconds { get; }

    /// <summary>
    /// Khởi tạo thông tin tài nguyên media cho quảng cáo.
    /// </summary>
    public AdMedia(string url, int durationInSeconds)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("URL quảng cáo không được để trống.");

        if (durationInSeconds <= 0)
            throw new DomainException("Thời lượng quảng cáo phải lớn hơn 0 giây.");

        Url = url.Trim();
        DurationInSeconds = durationInSeconds;
    }
}