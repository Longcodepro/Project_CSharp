namespace TuneVault.Domain.ValueObjects;

/// <summary>
/// Value Object đại diện cho thời lượng của một media item.
/// Đảm bảo Minutes và Seconds luôn hợp lệ và không âm.
/// </summary>
public record MediaDuration
{
    /// <summary>Số phút (>= 0).</summary>
    public int Minutes { get; }

    /// <summary>Số giây (0–59).</summary>
    public int Seconds { get; }

    /// <summary>
    /// Tổng thời lượng tính theo giây — dùng để lưu vào DB (cột DurationSeconds / TrailerSeconds).
    /// </summary>
    public int TotalSeconds => Minutes * 60 + Seconds;

    /// <summary>
    /// Khởi tạo một <see cref="MediaDuration"/> với số phút và giây.
    /// </summary>
    /// <param name="minutes">Số phút (phải >= 0).</param>
    /// <param name="seconds">Số giây (0 đến 59).</param>
    /// <exception cref="ArgumentException">Ném ra nếu minutes âm hoặc seconds ngoài khoảng 0–59.</exception>
    public MediaDuration(int minutes, int seconds)
    {
        if (minutes < 0) throw new ArgumentException("Số phút không được âm.");
        if (seconds < 0 || seconds >= 60) throw new ArgumentException("Số giây phải từ 0 đến 59.");

        Minutes = minutes;
        Seconds = seconds;
    }

    /// <summary>
    /// Tạo <see cref="MediaDuration"/> từ tổng số giây — dùng khi đọc từ database.
    /// </summary>
    /// <param name="totalSeconds">Tổng số giây (>= 0).</param>
    /// <returns>Instance <see cref="MediaDuration"/> tương ứng.</returns>
    public static MediaDuration FromSeconds(int totalSeconds)
    {
        if (totalSeconds < 0)
            throw new ArgumentException("Tổng số giây không được âm.");

        return new MediaDuration(totalSeconds / 60, totalSeconds % 60);
    }

    /// <summary>Chuyển sang <see cref="TimeSpan"/> để dùng trong các phép tính logic.</summary>
    public TimeSpan ToTimeSpan() => new TimeSpan(0, Minutes, Seconds);

    /// <summary>Hiển thị dạng MM:SS (VD: 03:45).</summary>
    public override string ToString() => $"{Minutes:D2}:{Seconds:D2}";
}
