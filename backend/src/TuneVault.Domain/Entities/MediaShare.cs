using System;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một hành động chia sẻ nội dung âm nhạc (bài hát, album, playlist) nội bộ giữa hai người dùng trong TuneVault.
/// Thực thể này đóng vai trò như một tin nhắn gửi gắm nội dung phương tiện đích danh từ người gửi đến người nhận.
/// </summary>
public class MediaShare
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MaxMessageLength = 250;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của bản ghi MediaShare. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người thực hiện hành động chia sẻ (Sender). Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string SenderId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người nhận nội dung chia sẻ (Receiver). Độ dài cố định từ 4 đến 5 ký tự (Không được phép null).
    /// </summary>
    public string ReceiverId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của mục nội dung được chia sẻ (Id của Bài hát, Album hoặc Playlist tương ứng). Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string SharedItemId { get; private set; } = string.Empty;

    /// <summary>
    /// Loại nội dung âm nhạc được chia sẻ (Sử dụng Enum ShareType).
    /// </summary>
    public ShareType ShareType { get; private set; }

    /// <summary>
    /// Lời nhắn hoặc nội dung văn bản ngắn đính kèm theo lượt chia sẻ (Tùy chọn).
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Mốc thời gian hệ thống ghi nhận hành động chia sẻ được thực hiện thành công.
    /// </summary>
    public DateTime SharedAt { get; private set; }

    // --- Constructors ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private MediaShare() { }

    /// <summary>
    /// Khởi tạo một bản ghi chia sẻ nội dung phương tiện nội bộ mới với các ràng buộc kiểm tra đối tác nghiêm ngặt.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của bản ghi MediaShare.</param>
    /// <param name="senderId">Mã người thực hiện hành động chia sẻ.</param>
    /// <param name="receiverId">Mã người nhận nội dung chia sẻ đích danh.</param>
    /// <param name="sharedItemId">Mã nội dung (bài hát/album/playlist) được chia sẻ.</param>
    /// <param name="shareType">Loại hình cấu trúc nội dung tương ứng.</param>
    /// <param name="message">Lời nhắn gửi kèm (tùy chọn).</param>
    /// <exception cref="DomainException">Ném ra khi vi phạm các quy tắc định danh hoặc người dùng tự chia sẻ cho chính mình.</exception>
    public MediaShare(string id, string senderId, string receiverId, string sharedItemId, ShareType shareType, string? message = null)
    {
        ValidateId(id);
        ValidateSenderId(senderId);
        ValidateReceiverId(receiverId);
        ValidateSharedItemId(sharedItemId);
        ValidateShareType(shareType);
        ValidateMessage(message);
        ValidateSelfSharing(senderId, receiverId);

        DateTime now = DateTime.UtcNow;
        ValidateSharedAt(now);

        Id = id.Trim();
        SenderId = senderId.Trim();
        ReceiverId = receiverId.Trim();
        SharedItemId = sharedItemId.Trim();
        ShareType = shareType;
        Message = message?.Trim();
        SharedAt = now;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh bản ghi MediaShare.
    /// </summary>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của MediaShare không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của MediaShare phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã người gửi (SenderId).
    /// </summary>
    private static void ValidateSenderId(string senderId)
    {
        if (string.IsNullOrWhiteSpace(senderId))
            throw new DomainException("Mã người gửi chia sẻ (SenderId) không được để trống.");

        int length = senderId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã người gửi chia sẻ phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã người nhận bắt buộc (ReceiverId).
    /// </summary>
    private static void ValidateReceiverId(string receiverId)
    {
        if (string.IsNullOrWhiteSpace(receiverId))
            throw new DomainException("Mã người nhận chia sẻ (ReceiverId) bắt buộc phải có và không được để trống.");

        int length = receiverId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã người nhận chia sẻ phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã nội dung âm nhạc (SharedItemId).
    /// </summary>
    private static void ValidateSharedItemId(string sharedItemId)
    {
        if (string.IsNullOrWhiteSpace(sharedItemId))
            throw new DomainException("Mã nội dung được chia sẻ (SharedItemId) không được để trống.");

        int length = sharedItemId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã nội dung được chia sẻ phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của loại hình nội dung được chia sẻ dựa trên tập hợp Enum đã định nghĩa.
    /// </summary>
    private static void ValidateShareType(ShareType shareType)
    {
        if (!Enum.IsDefined(typeof(ShareType), shareType))
            throw new DomainException("Loại hình nội dung chia sẻ (ShareType) không tồn tại trong hệ thống.");
    }

    /// <summary>
    /// Kiểm tra giới hạn độ dài của văn bản lời nhắn đi kèm tin nhắn chia sẻ.
    /// </summary>
    private static void ValidateMessage(string? message)
    {
        if (message != null && message.Trim().Length > MaxMessageLength)
            throw new DomainException($"Lời nhắn đính kèm không được phép vượt quá {MaxMessageLength} ký tự.");
    }

    /// <summary>
    /// Ngăn chặn hành vi lỗi logic nghiệp vụ khi người dùng cố tình tự gửi liên kết chia sẻ cho chính mình.
    /// </summary>
    private static void ValidateSelfSharing(string senderId, string receiverId)
    {
        if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
            return;

        if (senderId.Trim().Equals(receiverId.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Người gửi không thể tự chia sẻ nội dung âm nhạc cho chính bản thân mình.");
    }

    /// <summary>
    /// Xác thực tính toàn vẹn thời gian thực hiện hành động của hệ thống.
    /// </summary>
    private static void ValidateSharedAt(DateTime sharedAt)
    {
        if (sharedAt == default)
            throw new DomainException("Thời gian chia sẻ (SharedAt) không được mang giá trị mặc định.");

        if (sharedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian chia sẻ không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}