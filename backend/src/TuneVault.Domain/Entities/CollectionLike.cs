using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Lưu lượt thích của người dùng với album hoặc playlist trong cùng một bảng dữ liệu.
/// </summary>
public sealed class CollectionLike
{
    private const int MinIdLength = 4;
    private const int MaxIdLength = 10;

    /// <summary>
    /// Mã định danh duy nhất của lượt thích.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Người dùng thực hiện lượt thích.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã album hoặc playlist được thích.
    /// </summary>
    public string TargetId { get; private set; } = string.Empty;

    /// <summary>
    /// Loại đối tượng được thích để phân biệt TargetId thuộc Albums hay Playlists.
    /// </summary>
    public CollectionLikeTargetType TargetType { get; private set; }

    /// <summary>
    /// Thời điểm người dùng bấm thích.
    /// </summary>
    public DateTime LikedAt { get; private set; }

    private CollectionLike() { }

    /// <summary>
    /// Khởi tạo lượt thích album hoặc playlist mới.
    /// </summary>
    /// <param name="id">Mã định danh lượt thích.</param>
    /// <param name="userId">Mã người dùng.</param>
    /// <param name="targetId">Mã album hoặc playlist.</param>
    /// <param name="targetType">Loại đối tượng được thích.</param>
    public CollectionLike(string id, string userId, string targetId, CollectionLikeTargetType targetType)
    {
        ValidateId(id);
        ValidateId(userId, "UserId");
        ValidateId(targetId, "TargetId");
        ValidateTargetType(targetType);

        Id = id.Trim();
        UserId = userId.Trim();
        TargetId = targetId.Trim();
        TargetType = targetType;
        LikedAt = DateTime.UtcNow;
    }

    private static void ValidateId(string value, string name = "Id")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} của lượt thích không được để trống.");

        var length = value.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"{name} của lượt thích phải có độ dài từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    private static void ValidateTargetType(CollectionLikeTargetType targetType)
    {
        if (!Enum.IsDefined(typeof(CollectionLikeTargetType), targetType))
            throw new DomainException("Loại đối tượng được thích không hợp lệ.");
    }
}
