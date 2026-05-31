using System;

namespace TuneVault.Domain.Entities;

public class PlayHistory
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
    public float? StoppedAt { get; set; }
}
