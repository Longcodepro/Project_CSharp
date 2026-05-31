using System;

namespace TuneVault.Domain.Entities;

public class Follow
{
    public string FollowerId { get; set; } = string.Empty;
    public string FolloweeId { get; set; } = string.Empty;
    public DateTime FollowedAt { get; set; }
}
