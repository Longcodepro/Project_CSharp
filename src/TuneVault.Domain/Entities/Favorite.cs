using System;

namespace TuneVault.Domain.Entities;

public class Favorite
{
    public string UserId { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public string LikeStatus { get; set; } = string.Empty;
    public DateTime LikedAt { get; set; }
}
