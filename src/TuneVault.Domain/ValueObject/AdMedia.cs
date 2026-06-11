using System;

namespace TuneVault.Domain.ValueObjects;

public sealed class AdMedia
{
    public string Url { get; }
    public int DurationInSeconds { get; }

    public AdMedia(string url, int durationInSeconds)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url must not be empty", nameof(url));
        if (durationInSeconds < 0) throw new ArgumentOutOfRangeException(nameof(durationInSeconds));

        Url = url.Trim();
        DurationInSeconds = durationInSeconds;
    }
}
