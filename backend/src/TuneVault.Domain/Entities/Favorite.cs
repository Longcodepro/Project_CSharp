using System;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

public class Favorite
{
    private const int MinIdLength = 4;
    private const int MaxIdLength = 10;

    public string Id { get; set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string TargetId { get; private set; } = string.Empty;
    public FavoriteTargetType TargetType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime LikedAt { get; private set; }

    private Favorite() { }

    public Favorite(
        string id,
        string userId,
        string mediaItemId,
        DateTime? likedAt = null,
        bool isActive = true)
    {
        Initialize(id, userId, mediaItemId, FavoriteTargetType.Media, likedAt ?? DateTime.UtcNow, isActive);
    }

    public Favorite(
        string id,
        string userId,
        string targetId,
        FavoriteTargetType targetType,
        DateTime? likedAt = null,
        bool isActive = true)
    {
        Initialize(id, userId, targetId, targetType, likedAt ?? DateTime.UtcNow, isActive);
    }

    public void Activate()
    {
        IsActive = true;
        LikedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của bản ghi Favorite không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của bản ghi Favorite phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId trong danh sách yêu thích không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId trong danh sách yêu thích phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private void Initialize(
        string id,
        string userId,
        string targetId,
        FavoriteTargetType targetType,
        DateTime likedAt,
        bool isActive)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateTargetId(targetId);
        ValidateTargetType(targetType);
        ValidateLikedAt(likedAt);

        Id = id.Trim();
        UserId = userId.Trim();
        TargetId = targetId.Trim();
        TargetType = targetType;
        IsActive = isActive;
        LikedAt = likedAt;
    }

    private static void ValidateTargetId(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            throw new DomainException("TargetId trong danh sách yêu thích không được để trống.");

        int length = targetId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"TargetId trong danh sách yêu thích phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateTargetType(FavoriteTargetType targetType)
    {
        if (!Enum.IsDefined(typeof(FavoriteTargetType), targetType))
            throw new DomainException("Loại đối tượng yêu thích không hợp lệ.");
    }

    private static void ValidateLikedAt(DateTime likedAt)
    {
        if (likedAt == default)
            throw new DomainException("Thời gian yêu thích (LikedAt) không được mang giá trị mặc định.");

        if (likedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian yêu thích không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}
