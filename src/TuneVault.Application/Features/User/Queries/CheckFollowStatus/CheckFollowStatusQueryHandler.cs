using MediatR;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.CheckFollowStatus;

/// <summary>
/// Handler xử lý <see cref="CheckFollowStatusQuery"/>.
/// Delegate trực tiếp xuống repository để kiểm tra sự tồn tại của bản ghi quan hệ follow.
/// Không cần map sang DTO vì kết quả là giá trị nguyên thủy <c>bool</c>.
/// </summary>
public class CheckFollowStatusQueryHandler : IRequestHandler<CheckFollowStatusQuery, bool>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public CheckFollowStatusQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Kiểm tra trạng thái theo dõi:
    /// delegate xuống repository → trả về kết quả bool trực tiếp.
    /// </summary>
    /// <param name="request">Query chứa FollowerId và FolloweeId cần kiểm tra.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// <c>true</c> nếu FollowerId đang theo dõi FolloweeId; <c>false</c> nếu chưa.
    /// </returns>
    public async Task<bool> Handle(CheckFollowStatusQuery request, CancellationToken ct)
    {
        // Step 1: Delegate xuống repository — không cần xử lý thêm tại Application layer
        //         vì đây là pure query không có logic nghiệp vụ
        return await _userRepository.IsFollowingAsync(request.FollowerId, request.FolloweeId, ct);
    }
}
