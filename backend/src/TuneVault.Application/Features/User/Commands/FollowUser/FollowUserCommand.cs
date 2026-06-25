using MediatR;

namespace TuneVault.Application.Features.User.Commands.FollowUser;

/// <summary>
/// Command (yêu cầu ghi) đại diện cho nghiệp vụ theo dõi một người dùng khác.
/// Handler sẽ kiểm tra điều kiện hợp lệ, gọi method nghiệp vụ của Entity followee
/// để tăng <c>TotalFollowers</c>, persist Entity, và tạo bản ghi quan hệ trong bảng <c>UserFollows</c>.
/// </summary>
/// <param name="FollowerId">Mã định danh hệ thống của người thực hiện hành động theo dõi.</param>
/// <param name="FolloweeId">Mã định danh hệ thống của người được theo dõi.</param>
public record FollowUserCommand(
    string FollowerId,
    string FolloweeId) : IRequest<bool>;
