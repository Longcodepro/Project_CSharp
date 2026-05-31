using System;

namespace TuneVault.Domain.Entities;

public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
