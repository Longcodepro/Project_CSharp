using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.ValueObjects;

/// <summary>
/// Đối tượng giá trị (Value Object) cấu hình các năng lực, đặc quyền giới hạn đi kèm của một Hạng tài khoản.
/// Phục vụ cho việc kiểm tra quyền hạn của người dùng khi nghe hoặc tải nhạc.
/// </summary>
public class TierCapabilities
{
    // --- Constants ---
    private const int MinDownloadCount = 0;
    private const int MaxAudioQualityLength = 20;

    // --- Properties ---

    /// <summary>
    /// Số lượng bài hát tối đa được phép tải xuống thiết bị (0 nghĩa là không được tải).
    /// </summary>
    public int MaxDownloadTracks { get; private set; }

    /// <summary>
    /// Chất lượng âm thanh tối đa được trải nghiệm (Ví dụ: Standard, High, Lossless).
    /// </summary>
    public string AudioQuality { get; private set; }

    /// <summary>
    /// Trạng thái tắt quảng cáo khi nghe nhạc.
    /// </summary>
    public bool IsAdFree { get; private set; }

    /// <summary>
    /// Quyền chủ động chuyển bài hát không giới hạn.
    /// </summary>
    public bool CanSkipTracks { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng bắt buộc cho Dapper hoặc các ORM.
    /// </summary>
    private TierCapabilities() { }

    /// <summary>
    /// Khởi tạo bộ cấu hình năng lực đặc quyền cho hạng tài khoản.
    /// </summary>
    public TierCapabilities(int maxDownloadTracks, string audioQuality, bool isAdFree, bool canSkipTracks)
    {
        ValidateMaxDownloadTracks(maxDownloadTracks);
        ValidateAudioQuality(audioQuality);

        MaxDownloadTracks = maxDownloadTracks;
        AudioQuality = audioQuality.Trim();
        IsAdFree = isAdFree;
        CanSkipTracks = canSkipTracks;
    }

    // --- Business Methods ---

    /// <summary>
    /// Kiểm tra xem hạng tài khoản này có quyền lợi tải nhạc về máy hay không.
    /// </summary>
    public bool CanDownload()
    {
        if (MaxDownloadTracks > 0)
        {
            return true;
        }

        return false;
    }

    // --- Validation Methods ---

    private static void ValidateMaxDownloadTracks(int maxDownloadTracks)
    {
        if (maxDownloadTracks < MinDownloadCount)
            throw new DomainException("Giới hạn số lượng bài hát tải xuống không được nhỏ hơn 0.");
    }

    private static void ValidateAudioQuality(string audioQuality)
    {
        if (string.IsNullOrWhiteSpace(audioQuality))
            throw new DomainException("Thông tin chất lượng âm thanh cấu hình không được để trống.");

        string normalizedQuality = audioQuality.Trim();
        if (normalizedQuality.Length > MaxAudioQualityLength)
            throw new DomainException($"Tên cấu hình chất lượng âm thanh không được vượt quá {MaxAudioQualityLength} ký tự.");
    }
}