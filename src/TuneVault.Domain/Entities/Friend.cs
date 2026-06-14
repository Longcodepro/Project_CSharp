using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho mối quan hệ bạn bè (Friend) hai chiều giữa hai người dùng trong hệ thống TuneVault.
/// Thực thể độc lập này tự quản lý trạng thái lời mời, các bước chuyển vòng đời quan hệ và quy tắc xác thực liên quan.
/// </summary>
public class Friend
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của bản ghi Friend. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người dùng chủ động gửi lời mời kết bạn (RequestedBy). Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string RequestedById { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người dùng nhận được lời mời kết bạn (RequestedTo). Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string RequestedToId { get; private set; } = string.Empty;

    /// <summary>
    /// Trạng thái hiện tại của mối quan hệ bạn bè (Pending, Accepted, Blocked).
    /// </summary>
    public FriendStatus Status { get; private set; }

    /// <summary>
    /// Mốc thời gian hệ thống ghi nhận lúc mối quan hệ/lời mời kết bạn này được khởi tạo.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private Friend() { }

    /// <summary>
    /// Khởi tạo một lời mời kết bạn mới thiết lập mặc định ở trạng thái chờ duyệt (Pending).
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của bản ghi Friend.</param>
    /// <param name="requestedById">Mã định danh của người gửi lời mời.</param>
    /// <param name="requestedToId">Mã định danh của người nhận lời mời.</param>
    /// <exception cref="DomainException">Ném ra khi vi phạm các quy tắc định dạng hoặc người dùng tự kết bạn với chính mình.</exception>
    public Friend(string id, string requestedById, string requestedToId)
    {
        ValidateId(id);
        ValidateRequestedById(requestedById);
        ValidateRequestedToId(requestedToId);
        ValidateSelfFriending(requestedById, requestedToId);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        RequestedById = requestedById.Trim();
        RequestedToId = requestedToId.Trim();
        Status = FriendStatus.Pending; // Mặc định khi gửi lời mời là Pending
        CreatedAt = now;
    }

    // --- Business Methods ---

    /// <summary>
    /// Chấp nhận lời mời kết bạn, chính thức thiết lập mối quan hệ bạn bè hai chiều (Chuyển trạng thái sang Accepted).
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu lời mời hiện tại không ở trạng thái Pending.</exception>
    public void AcceptFriendRequest()
    {
        if (Status != FriendStatus.Pending)
            throw new DomainException("Chỉ có thể chấp nhận lời mời kết bạn khi mối quan hệ đang ở trạng thái chờ xử lý (Pending).");

        Status = FriendStatus.Accepted;
    }

    /// <summary>
    /// Chặn người dùng còn lại trong mối quan hệ kết bạn, cắt đứt tương tác giữa hai bên (Chuyển trạng thái sang Blocked).
    /// </summary>
    public void BlockFriend()
    {
        Status = FriendStatus.Blocked;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh bản ghi Friend.
    /// </summary>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của bản ghi Friend không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của bản ghi Friend phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã người gửi lời mời (RequestedById).
    /// </summary>
    private static void ValidateRequestedById(string requestedById)
    {
        if (string.IsNullOrWhiteSpace(requestedById))
            throw new DomainException("Mã người gửi lời mời kết bạn (RequestedById) không được để trống.");

        int length = requestedById.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã người gửi lời mời kết bạn phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã người nhận lời mời (RequestedToId).
    /// </summary>
    private static void ValidateRequestedToId(string requestedToId)
    {
        if (string.IsNullOrWhiteSpace(requestedToId))
            throw new DomainException("Mã người nhận lời mời kết bạn (RequestedToId) không được để trống.");

        int length = requestedToId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã người nhận lời mời kết bạn phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra ràng buộc nghiệp vụ bất biến: Ngăn chặn tuyệt đối hành vi người dùng tự gửi lời mời kết bạn cho chính mình.
    /// </summary>
    private static void ValidateSelfFriending(string requestedById, string requestedToId)
    {
        if (string.IsNullOrEmpty(requestedById) || string.IsNullOrEmpty(requestedToId))
            return;

        if (requestedById.Trim().Equals(requestedToId.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Người dùng không thể tự gửi lời mời kết bạn cho chính bản thân.");
    }

    /// <summary>
    /// Xác thực tính toàn vẹn của mốc thời gian hệ thống khi khởi tạo mối quan hệ bạn bè.
    /// </summary>
    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian khởi tạo kết bạn (CreatedAt) không được mang giá trị mặc định.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian khởi tạo kết bạn không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}