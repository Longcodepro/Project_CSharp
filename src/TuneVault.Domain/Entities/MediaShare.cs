using System;

namespace TuneVault.Domain.Entities;

public class MediaShare
{
    public string Id { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ShareType { get; set; } = string.Empty;
    public string SharedItemId { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public bool IsRead { get; set; }
}
