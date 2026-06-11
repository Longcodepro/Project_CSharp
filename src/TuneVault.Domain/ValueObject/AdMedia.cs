using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.ValueObjects;

/// <summary>
/// Đối tượng giá trị (Value Object) quản lý thông tin tài nguyên truyền thông của quảng cáo.
/// Đảm bảo tính toàn vẹn của đường dẫn URL và thời lượng phát sóng.
/// </summary>
public record AdMedia
{
    /// <summary>
    /// Đường dẫn liên kết (URL) đến tệp tin cấu hình tài nguyên quảng cáo công cộng.
    /// </summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>
    /// Thời lượng phát sóng của nội dung quảng cáo (tính bằng đơn vị Giây).
    /// </summary>
    public int DurationInSeconds { get; private set; }

    /// <summary>
    /// Constructor rỗng cấu hình quyền private phục vụ cơ chế mapping tự động của Dapper.
    /// </summary>
    private AdMedia() { }

    /// <summary>
    /// Khởi tạo một đối tượng dữ liệu truyền thông quảng cáo mới đi kèm các điều kiện xác thực nghiêm ngặt.
    /// </summary>
    /// <param name="url">Đường dẫn tài nguyên âm thanh/hình ảnh quảng cáo.</param>
    /// <param name="durationInSeconds">Thời lượng phát quảng cáo (phải lớn hơn 0).</param>
    /// <exception cref="DomainException">Ném ra khi URL trống hoặc thời lượng không hợp lệ.</exception>
    public AdMedia(string url, int durationInSeconds)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Đường dẫn URL tài nguyên quảng cáo không được để trống.");

        if (durationInSeconds <= 0)
            throw new DomainException("Thời lượng phát hành quảng cáo phải lớn hơn 0 giây.");

        Url = url.Trim();
        DurationInSeconds = durationInSeconds;
    }
}