using System;

namespace TuneVault.Domain.Entities;

public class Playlist
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CoverImgUrl { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
}
