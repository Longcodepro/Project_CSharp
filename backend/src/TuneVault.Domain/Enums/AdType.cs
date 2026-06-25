namespace TuneVault.Domain.Enums;

/// <summary>
/// Định nghĩa các loại hình thức quảng cáo hiển thị hoặc phát trong hệ thống TuneVault.
/// </summary>
public enum AdType
{
    /// <summary>
    /// Quảng cáo âm thanh chèn ngắt quãng giữa các bài hát (Dành cho tài khoản Free).
    /// </summary>
    AudioInterrupt = 1,

    /// <summary>
    /// Quảng cáo video phát trước hoặc trong quá trình trải nghiệm ứng dụng.
    /// </summary>
    VideoRoll = 2,

    /// <summary>
    /// Quảng cáo dạng biểu ngữ hiển thị trên các khu vực giao diện.
    /// </summary>
    Banner = 3
}