using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một bản ghi Yêu thích/Tương tác cảm xúc (Favorite) của người dùng đối với bài hát trong TuneVault.
/// Thực thể độc lập này tự quản lý thông tin tương tác, loại cảm xúc và các quy tắc xác thực liên quan.
/// </summary>
public class Favorite
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của bản ghi Favorite. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người dùng (User) thực hiện hành động yêu thích. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của vật phẩm phương tiện (MediaItem) được yêu thích. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string MediaItemId { get; private set; } = string.Empty;

    /// <summary>
    /// Trạng thái cảm xúc cụ thể được lựa chọn (Sử dụng Enum định nghĩa sẵn).
    /// </summary>
    public FavoriteReaction Reaction { get; private set; }

    /// <summary>
    /// Thời điểm người dùng thực hiện hành động bày tỏ cảm xúc / yêu thích này.
    /// </summary>
    public DateTime LikedAt { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private Favorite() { }

    /// <summary>
    /// Khởi tạo một bản ghi tương tác yêu thích mới với các ràng buộc khắt khe về thông tin định danh và cảm xúc hợp lệ.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của bản ghi Favorite.</param>
    /// <param name="userId">Mã định danh của người dùng tương tác.</param>
    /// <param name="mediaItemId">Mã định danh của bài hát được tương tác.</param>
    /// <param name="reaction">Trạng thái cảm xúc lựa chọn (Mặc định là Thích - Like).</param>
    public Favorite(string id, string userId, string mediaItemId, FavoriteReaction reaction = FavoriteReaction.Like)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateMediaItemId(mediaItemId);
        ValidateReaction(reaction);

        DateTime now = DateTime.UtcNow;
        ValidateLikedAt(now);

        Id = id.Trim();
        UserId = userId.Trim();
        MediaItemId = mediaItemId.Trim();
        Reaction = reaction;
        LikedAt = now;
    }

    // --- Business Methods ---

    /// <summary>
    /// Thực hiện cập nhật hoặc thay đổi trạng thái cảm xúc của người dùng đối với bài hát (ví dụ: từ Like sang Love hoặc Chill).
    /// </summary>
    /// <param name="newReaction">Trạng thái cảm xúc mới cần cập nhật.</param>
    public void UpdateReaction(FavoriteReaction newReaction)
    {
        ValidateReaction(newReaction);
        Reaction = newReaction;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh bản ghi Favorite.
    /// </summary>
    /// <param name="id">Chuỗi định danh cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi Id trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của bản ghi Favorite không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của bản ghi Favorite phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh người dùng (UserId).
    /// </summary>
    /// <param name="userId">Chuỗi định danh người dùng cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi UserId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId trong danh sách yêu thích không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId trong danh sách yêu thích phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh vật phẩm phương tiện (MediaItemId).
    /// </summary>
    /// <param name="mediaItemId">Chuỗi định danh MediaItem cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi MediaItemId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateMediaItemId(string mediaItemId)
    {
        if (string.IsNullOrWhiteSpace(mediaItemId))
            throw new DomainException("MediaItemId trong danh sách yêu thích không được để trống.");

        int length = mediaItemId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"MediaItemId trong danh sách yêu thích phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của giá trị Enum cảm xúc truyền vào hệ thống, ngăn chặn các giá trị số nằm ngoài vùng định nghĩa.
    /// </summary>
    /// <param name="reaction">Giá trị thuộc Enum FavoriteReaction cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi giá trị Enum không tồn tại hoặc không hợp lệ.</exception>
    private static void ValidateReaction(FavoriteReaction reaction)
    {
        if (!Enum.IsDefined(typeof(FavoriteReaction), reaction))
            throw new DomainException("Trạng thái cảm xúc yêu thích (FavoriteReaction) không hợp lệ.");
    }

    /// <summary>
    /// Xác thực tính toàn vẹn của mốc thời gian hệ thống ghi nhận lúc người dùng tương tác yêu thích.
    /// </summary>
    /// <param name="likedAt">Mốc thời gian DateTime cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi mốc thời gian mang giá trị mặc định hoặc vượt quá thời gian hiện tại.</exception>
    private static void ValidateLikedAt(DateTime likedAt)
    {
        if (likedAt == default)
            throw new DomainException("Thời gian yêu thích (LikedAt) không được mang giá trị mặc định.");

        if (likedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian yêu thích không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}