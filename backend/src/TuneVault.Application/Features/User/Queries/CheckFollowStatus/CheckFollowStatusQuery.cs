using MediatR;

namespace TuneVault.Application.Features.User.Queries.CheckFollowStatus;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ kiểm tra trạng thái theo dõi giữa hai người dùng.
/// Dùng để hiển thị nút "Follow" hay "Unfollow" trên giao diện.
/// Kết quả trả về là <c>bool</c> — không cần DTO vì chỉ là trạng thái nhị phân.
/// </summary>
/// <param name="FollowerId">Mã định danh hệ thống của người thực hiện theo dõi (người dùng hiện tại).</param>
/// <param name="FolloweeId">Mã định danh hệ thống của người được kiểm tra (profile đang xem).</param>
public record CheckFollowStatusQuery(
    string FollowerId,
    string FolloweeId) : IRequest<bool>;
