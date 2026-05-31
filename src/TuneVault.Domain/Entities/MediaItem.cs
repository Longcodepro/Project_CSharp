using System;

namespace TuneVault.Domain.Entities;

public class MediaItem
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string? CoverImgUrl { get; set; }
    public string? CanvasUrl { get; set; }
    public float Duration { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime ReleaseDate { get; set; }
    public int ViewCount { get; set; }
}
