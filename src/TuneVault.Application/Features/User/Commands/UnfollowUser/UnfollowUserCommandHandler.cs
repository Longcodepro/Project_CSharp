using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.UnfollowUser;

/// <summary>
/// Handler xử lý <see cref="UnfollowUserCommand"/>.
/// Luồng xử lý: kiểm tra quan hệ follow tồn tại → gọi <c>DecrementFollowers()</c>
/// trên Entity followee → persist Entity → xóa bản ghi UserFollows.
/// </summary>
public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public UnfollowUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng hủy theo dõi người dùng theo thứ tự:
    /// guard clauses → kiểm tra quan hệ → gọi method Entity → persist Entity → xóa bản ghi quan hệ.
    /// </summary>
    /// <param name="request">Command chứa FollowerId và FolloweeId.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu toàn bộ thao tác thành công.</returns>
    /// <exception cref="DomainException">
    /// Ném ra nếu: User không tồn tại, hoặc quan hệ follow không tồn tại.
    /// </exception>
    public async Task<bool> Handle(UnfollowUserCommand request, CancellationToken ct)
    {
        // Step 1: Lấy Entity của người được theo dõi (cần để giảm TotalFollowers)
        var followee = await _userRepository.GetByIdAsync(request.FolloweeId, ct);

        // Step 2: Kiểm tra sự tồn tại của followee
        if (followee is null)
            throw new DomainException($"Người dùng với Id '{request.FolloweeId}' không tồn tại.");

        // Step 3: Kiểm tra quan hệ follow có tồn tại không — tránh thao tác vô nghĩa
        var isFollowing = await _userRepository.IsFollowingAsync(request.FollowerId, request.FolloweeId, ct);
        if (!isFollowing)
            throw new DomainException("Bạn chưa theo dõi người dùng này.");

        // Step 4: Gọi method nghiệp vụ của Entity để giảm TotalFollowers (không dưới 0)
        followee.DecrementFollowers();

        // Step 5: Persist trạng thái Entity followee (TotalFollowers đã giảm) vào DB
        await _userRepository.UpdateAsync(followee, ct);

        // Step 6: Xóa bản ghi quan hệ follow khỏi bảng UserFollows và trả về kết quả
        return await _userRepository.UnfollowUserAsync(request.FollowerId, request.FolloweeId, ct);
    }
}
