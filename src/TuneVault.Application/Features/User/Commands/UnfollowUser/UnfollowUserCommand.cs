using MediatR;

namespace TuneVault.Application.Features.User.Commands.UnfollowUser;

/// <summary>
/// Command (yêu cầu ghi) đại diện cho nghiệp vụ hủy theo dõi một người dùng khác.
/// Handler sẽ kiểm tra quan hệ tồn tại, gọi <c>DecrementFollowers()</c> trên Entity followee,
/// persist Entity, rồi xóa bản ghi khỏi bảng <c>UserFollows</c>.
/// </summary>
/// <param name="FollowerId">Mã định danh hệ thống của người thực hiện hủy theo dõi.</param>
/// <param name="FolloweeId">Mã định danh hệ thống của người bị hủy theo dõi.</param>
public record UnfollowUserCommand(
    string FollowerId,
    string FolloweeId) : IRequest<bool>;
