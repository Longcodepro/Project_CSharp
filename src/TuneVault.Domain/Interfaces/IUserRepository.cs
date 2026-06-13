using TuneVault.Domain.Entities;

namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Giao diện kho dữ liệu (Repository Interface) cho thực thể <see cref="User"/>.
/// Định nghĩa toàn bộ các thao tác truy vấn (Query) và lệnh (Command) của module User.
/// Tầng Domain chỉ phụ thuộc vào interface này; tầng Infrastructure sẽ cung cấp triển khai cụ thể.
/// </summary>
public interface IUserRepository
{
    // =========================================================================
    // QUERIES
    // =========================================================================

    /// <summary>
    /// Lấy thông tin đầy đủ của <see cref="User"/> theo mã định danh hệ thống (Primary Key).
    /// </summary>
    /// <param name="id">Mã định danh hệ thống của User (ví dụ: U001).</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns>
    /// Đối tượng <see cref="User"/> nếu tìm thấy; <c>null</c> nếu không tồn tại bản ghi tương ứng.
    /// </returns>
    Task<User?> GetByIdAsync(string id, CancellationToken ct);

    /// <summary>
    /// Lấy thông tin đầy đủ của <see cref="User"/> theo handle hiển thị công khai (IdDisplay).
    /// </summary>
    /// <param name="idDisplay">Handle công khai của User (ví dụ: john_doe).</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns>
    /// Đối tượng <see cref="User"/> nếu tìm thấy; <c>null</c> nếu không tồn tại bản ghi tương ứng.
    /// </returns>
    Task<User?> GetByIdDisplayAsync(string idDisplay, CancellationToken ct);

    /// <summary>
    /// Lấy danh sách tất cả <see cref="User"/> có trạng thái <c>IsArtist = true</c>.
    /// </summary>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns>Tập hợp các <see cref="User"/> Entity là nghệ sĩ.</returns>
    Task<IEnumerable<User>> GetAllArtistsAsync(CancellationToken ct);

    /// <summary>
    /// Lấy danh sách những <see cref="User"/> đang theo dõi tài khoản được chỉ định (followers của followeeId).
    /// </summary>
    /// <param name="followeeId">Mã định danh của User được theo dõi.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns>Tập hợp các <see cref="User"/> Entity đang theo dõi followeeId.</returns>
    Task<IEnumerable<User>> GetFollowersAsync(string followeeId, CancellationToken ct);

    /// <summary>
    /// Lấy danh sách những <see cref="User"/> mà tài khoản được chỉ định đang theo dõi (following của followerId).
    /// </summary>
    /// <param name="followerId">Mã định danh của User đang thực hiện hành động theo dõi.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns>Tập hợp các <see cref="User"/> Entity mà followerId đang theo dõi.</returns>
    Task<IEnumerable<User>> GetFollowingAsync(string followerId, CancellationToken ct);

    /// <summary>
    /// Kiểm tra xem <paramref name="followerId"/> có đang theo dõi <paramref name="followeeId"/> hay không.
    /// </summary>
    /// <param name="followerId">Mã định danh của người thực hiện theo dõi.</param>
    /// <param name="followeeId">Mã định danh của người được theo dõi.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns><c>true</c> nếu quan hệ follow tồn tại; <c>false</c> nếu chưa theo dõi.</returns>
    Task<bool> IsFollowingAsync(string followerId, string followeeId, CancellationToken ct);

    /// <summary>
    /// Kiểm tra xem một <see cref="User"/> có tồn tại trong hệ thống hay không dựa vào Id.
    /// </summary>
    /// <param name="id">Mã định danh hệ thống cần kiểm tra.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns><c>true</c> nếu tồn tại; <c>false</c> nếu không.</returns>
    Task<bool> ExistsAsync(string id, CancellationToken ct);

    // =========================================================================
    // COMMANDS
    // =========================================================================

    /// <summary>
    /// Lưu toàn bộ trạng thái hiện tại của <see cref="User"/> Entity vào cơ sở dữ liệu.
    /// Phương thức này chỉ được gọi sau khi các phương thức nghiệp vụ của Entity đã được thực thi
    /// (ví dụ: <c>user.UpdateProfile(...)</c>, <c>user.VerifyAsArtist()</c>).
    /// </summary>
    /// <param name="user">Đối tượng <see cref="User"/> đã được cập nhật trạng thái bởi tầng Application.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns><c>true</c> nếu cập nhật thành công (ít nhất 1 row bị ảnh hưởng); <c>false</c> nếu không.</returns>
    Task<bool> UpdateAsync(User user, CancellationToken ct);

    /// <summary>
    /// Tạo bản ghi quan hệ theo dõi (follow) giữa hai người dùng trong bảng <c>UserFollows</c>.
    /// </summary>
    /// <param name="followerId">Mã định danh của người thực hiện theo dõi.</param>
    /// <param name="followeeId">Mã định danh của người được theo dõi.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns><c>true</c> nếu thêm thành công; <c>false</c> nếu không có hàng nào được thêm.</returns>
    Task<bool> FollowUserAsync(string followerId, string followeeId, CancellationToken ct);

    /// <summary>
    /// Xóa bản ghi quan hệ theo dõi (unfollow) giữa hai người dùng khỏi bảng <c>UserFollows</c>.
    /// </summary>
    /// <param name="followerId">Mã định danh của người thực hiện hủy theo dõi.</param>
    /// <param name="followeeId">Mã định danh của người bị hủy theo dõi.</param>
    /// <param name="ct">Token dùng để hủy tác vụ bất đồng bộ khi cần thiết.</param>
    /// <returns><c>true</c> nếu xóa thành công; <c>false</c> nếu bản ghi không tồn tại.</returns>
    Task<bool> UnfollowUserAsync(string followerId, string followeeId, CancellationToken ct);
}
