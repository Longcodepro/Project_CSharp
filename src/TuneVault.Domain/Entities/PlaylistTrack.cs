using System;

namespace TuneVault.Domain.Entities;

public class PlaylistTrack
{
    public string PlaylistId { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public int TrackOrder { get; set; }
    public DateTime AddedAt { get; set; }
}
