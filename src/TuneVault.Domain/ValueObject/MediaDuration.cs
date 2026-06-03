namespace TuneVault.Domain.ValueObjects;

public record MediaDuration
{
    public int Minutes { get; }
    public int Seconds { get; }

    public MediaDuration(int minutes, int seconds)
    {
        if (minutes < 0) throw new ArgumentException("Số phút không được âm.");
        if (seconds < 0 || seconds >= 60) throw new ArgumentException("Số giây phải từ 0 đến 59.");
        
        Minutes = minutes;
        Seconds = seconds;
    }

    // Chuyển đổi sang TimeSpan để dùng cho các logic tính toán của C#
    public TimeSpan ToTimeSpan() => new TimeSpan(0, Minutes, Seconds);

    public override string ToString() => $"{Minutes:D2}:{Seconds:D2}";
}