using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho mối quan hệ theo dõi (Follow) giữa hai người dùng trong hệ thống TuneVault.
/// Thực thể độc lập này tự quản lý cờ trạng thái hoạt động (Soft Delete) và các quy tắc xác thực liên quan.
/// </summary>
public class Follow
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của bản ghi Follow. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Mã định danh của người dùng thực hiện hành động bấm theo dõi (Follower). Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string FollowerId { get; private set; }

    /// <summary>
    /// Mã định danh của người dùng/nghệ sĩ được theo dõi (Followee). Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string FolloweeId { get; private set; }

    /// <summary>
    /// Thời điểm hành động theo dõi (hoặc tái theo dõi) được thiết lập thành công.
    /// </summary>
    public DateTime FollowedAt { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động của mối quan hệ theo dõi. 
    /// Giá trị true biểu thị đang theo dõi, false biểu thị đã hủy theo dõi (Soft Delete).
    /// </summary>
    public bool IsActive { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private Follow() { }

    /// <summary>
    /// Khởi tạo một mối quan hệ theo dõi mới với trạng thái kích hoạt mặc định là hoạt động.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của bản ghi Follow.</param>
    /// <param name="followerId">Mã định danh của người đi theo dõi.</param>
    /// <param name="followeeId">Mã định danh của người được theo dõi.</param>
    /// <exception cref="DomainException">Ném ra khi dữ liệu đầu vào không hợp lệ hoặc người dùng tự theo dõi chính mình.</exception>
    public Follow(string id, string followerId, string followeeId)
    {
        ValidateId(id);
        ValidateFollowerId(followerId);
        ValidateFolloweeId(followeeId);
        ValidateSelfFollowing(followerId, followeeId);
        
        DateTime now = DateTime.UtcNow;
        ValidateFollowedAt(now);

        Id = id.Trim();
        FollowerId = followerId.Trim();
        FolloweeId = followeeId.Trim();
        FollowedAt = now;
        IsActive = true; // Mặc định khi tạo mới là trạng thái đang theo dõi
    }

    // --- Business Methods ---

    /// <summary>
    /// Thực hiện hành động hủy theo dõi (Unfollow) bằng cơ chế Soft Delete, chuyển cờ IsActive về false.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu mối quan hệ này vốn dĩ đã ở trạng thái hủy theo dõi từ trước.</exception>
    public void Unfollow()
    {
        if (!IsActive)
            throw new DomainException("Mối quan hệ theo dõi này đã được hủy từ trước đó.");

        IsActive = false;
    }

    /// <summary>
    /// Thực hiện hành động tái theo dõi lại (Refollow) đối với một mối quan hệ cũ đã từng bị hủy.
    /// Phương thức này kích hoạt lại cờ IsActive và làm mới mốc thời gian FollowedAt về thời điểm hiện tại.
    /// </summary>
    /// <exception cref="DomainException">Ném ra nếu người dùng hiện tại đang trong trạng thái theo dõi rồi.</exception>
    public void Refollow()
    {
        if (IsActive)
            throw new DomainException("Người dùng hiện đã và đang theo dõi mục tiêu này rồi.");

        DateTime now = DateTime.UtcNow;
        ValidateFollowedAt(now);

        FollowedAt = now;
        IsActive = true;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh bản ghi Follow.
    /// </summary>
    /// <param name="id">Chuỗi định danh cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi Id trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của bản ghi Follow không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của bản ghi Follow phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã người đi theo dõi (FollowerId).
    /// </summary>
    /// <param name="followerId">Chuỗi định danh Follower cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi FollowerId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateFollowerId(string followerId)
    {
        if (string.IsNullOrWhiteSpace(followerId))
            throw new DomainException("Mã người theo dõi (FollowerId) không được để trống.");

        int length = followerId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã người theo dõi (FollowerId) phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã người được theo dõi (FolloweeId).
    /// </summary>
    /// <param name="followeeId">Chuỗi định danh Followee cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi FolloweeId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateFolloweeId(string followeeId)
    {
        if (string.IsNullOrWhiteSpace(followeeId))
            throw new DomainException("Mã người được theo dõi (FolloweeId) không được để trống.");

        int length = followeeId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Mã người được theo dõi (FolloweeId) phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Kiểm tra ràng buộc nghiệp vụ tối cao: Ngăn chặn tuyệt đối hành vi người dùng tự theo dõi chính bản thân mình.
    /// </summary>
    /// <param name="followerId">Mã người đi theo dõi.</param>
    /// <param name="followeeId">Mã người được theo dõi.</param>
    /// <exception cref="DomainException">Ném ra khi hai mã định danh trùng nhau (không phân biệt hoa thường).</exception>
    private static void ValidateSelfFollowing(string followerId, string followeeId)
    {
        if (string.IsNullOrEmpty(followerId) || string.IsNullOrEmpty(followeeId))
            return;

        if (followerId.Trim().Equals(followeeId.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Người dùng không được phép tự theo dõi chính mình.");
    }

    /// <summary>
    /// Xác thực tính toàn vẹn của mốc thời gian hệ thống ghi nhận lúc thiết lập mối quan hệ theo dõi.
    /// </summary>
    /// <param name="followedAt">Mốc thời gian DateTime cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi mốc thời gian mang giá trị mặc định hoặc vượt quá thời gian hiện tại.</exception>
    private static void ValidateFollowedAt(DateTime followedAt)
    {
        if (followedAt == default)
            throw new DomainException("Thời gian theo dõi (FollowedAt) không được mang giá trị mặc định.");

        if (followedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian theo dõi không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}