using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.FollowUser;

/// <summary>
/// Handler xử lý <see cref="FollowUserCommand"/>.
/// Luồng xử lý: kiểm tra tự follow → kiểm tra tồn tại → kiểm tra đã follow rồi chưa
/// → gọi <c>IncrementFollowers()</c> trên Entity → persist Entity → tạo bản ghi UserFollows.
/// </summary>
public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public FollowUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng theo dõi người dùng theo thứ tự:
    /// guard clauses → gọi method Entity → persist Entity → tạo bản ghi quan hệ.
    /// </summary>
    /// <param name="request">Command chứa FollowerId và FolloweeId.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu toàn bộ thao tác thành công.</returns>
    /// <exception cref="DomainException">
    /// Ném ra nếu: tự follow, User không tồn tại, hoặc đã follow rồi.
    /// </exception>
    public async Task<bool> Handle(FollowUserCommand request, CancellationToken ct)
    {
        // Step 1: Guard clause — không cho phép tự follow chính mình
        if (request.FollowerId == request.FolloweeId)
            throw new DomainException("Người dùng không thể tự theo dõi chính mình.");

        // Step 2: Lấy cả hai User Entity đồng thời để tối ưu hiệu năng
        var follower = await _userRepository.GetByIdAsync(request.FollowerId, ct);
        var followee = await _userRepository.GetByIdAsync(request.FolloweeId, ct);

        // Step 3: Kiểm tra sự tồn tại của cả hai tài khoản
        if (follower is null)
            throw new DomainException($"Người dùng với Id '{request.FollowerId}' không tồn tại.");
        if (followee is null)
            throw new DomainException($"Người dùng với Id '{request.FolloweeId}' không tồn tại.");

        // Step 4: Kiểm tra quan hệ follow đã tồn tại chưa — tránh duplicate
        var alreadyFollowing = await _userRepository.IsFollowingAsync(request.FollowerId, request.FolloweeId, ct);
        if (alreadyFollowing)
            throw new DomainException("Bạn đã theo dõi người dùng này rồi.");

        // Step 5: Gọi method nghiệp vụ của Entity followee để tăng TotalFollowers
        followee.IncrementFollowers();

        // Step 6: Persist trạng thái Entity followee (TotalFollowers đã tăng) vào DB
        await _userRepository.UpdateAsync(followee, ct);

        // Step 7: Tạo bản ghi quan hệ follow trong bảng UserFollows
        return await _userRepository.FollowUserAsync(request.FollowerId, request.FolloweeId, ct);
    }
}
