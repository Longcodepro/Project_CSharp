using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.CheckFollowStatus;

/// <summary>
/// Handler xử lý <see cref="CheckFollowStatusQuery"/>.
/// Delegate trực tiếp xuống repository để kiểm tra sự tồn tại của bản ghi quan hệ follow.
/// Không cần map sang DTO vì kết quả là giá trị nguyên thủy <c>bool</c>.
/// Phân quyền: chỉ Listener / Artist / Admin đã đăng nhập và FollowerId phải là chính người dùng hiện tại.
/// </summary>
public class CheckFollowStatusQueryHandler : IRequestHandler<CheckFollowStatusQuery, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public CheckFollowStatusQueryHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Kiểm tra trạng thái theo dõi:
    /// kiểm tra xác thực & quyền sở hữu → delegate xuống repository → trả về kết quả bool trực tiếp.
    /// </summary>
    /// <param name="request">Query chứa FollowerId và FolloweeId cần kiểm tra.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// <c>true</c> nếu FollowerId đang theo dõi FolloweeId; <c>false</c> nếu chưa.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu FollowerId khác với người dùng hiện tại.</exception>
    public async Task<bool> Handle(CheckFollowStatusQuery request, CancellationToken ct)
    {
        // Step 0: Kiểm tra đã xác thực chưa
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập để kiểm tra trạng thái theo dõi.");

        // Step 0.1: Chỉ cho phép người dùng kiểm tra trạng thái theo dõi của chính mình
        if (!currentUserId.Equals(request.FollowerId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền kiểm tra trạng thái theo dõi của người dùng khác.");

        // Step 1: Delegate xuống repository — không cần xử lý thêm tại Application layer
        //         vì đây là pure query không có logic nghiệp vụ
        return await _userRepository.IsFollowingAsync(request.FollowerId, request.FolloweeId, ct);
    }
}