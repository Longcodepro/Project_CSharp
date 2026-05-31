namespace TuneVault.Domain.Entities;

public class AlbumTrack
{
    public string AlbumId { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public int TrackOrder { get; set; }
}
