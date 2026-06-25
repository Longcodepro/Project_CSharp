using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.UpdateSecurity;

/// <summary>
/// Handler xử lý <see cref="UpdateSecurityCommand"/>.
/// Điều phối luồng thay đổi mật khẩu: kiểm tra xác thực & quyền sở hữu → lấy Entity → gọi <c>ChangePassword</c> → persist.
/// Không trả về DTO vì đây là thao tác bảo mật — không cần expose dữ liệu nhạy cảm.
/// Phân quyền: chỉ Listener / Artist / Admin đã đăng nhập và đang đổi mật khẩu của chính mình.
/// </summary>
public class UpdateSecurityCommandHandler : IRequestHandler<UpdateSecurityCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public UpdateSecurityCommandHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng cập nhật mật khẩu:
    /// kiểm tra xác thực & quyền sở hữu → lấy Entity → gọi method <c>ChangePassword</c> của Entity → persist.
    /// </summary>
    /// <param name="request">Command chứa Id và PasswordHash mới đã được mã hóa.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu cập nhật thành công.</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu Id khác với người dùng hiện tại.</exception>
    /// <exception cref="DomainException">Ném ra nếu User không tồn tại hoặc hash không hợp lệ.</exception>
    public async Task<bool> Handle(UpdateSecurityCommand request, CancellationToken ct)
    {
        // Step 0: Kiểm tra đã xác thực chưa
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi cập nhật bảo mật.");

        // Step 0.1: Chỉ cho phép người dùng đổi mật khẩu của chính mình
        if (!currentUserId.Equals(request.Id, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền thay đổi mật khẩu của người dùng khác.");

        // Step 1: Lấy User Entity từ repository theo Id hệ thống
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        // Step 2: Kiểm tra sự tồn tại — ném exception nếu không tìm thấy
        if (user is null)
            throw new DomainException($"Người dùng với Id '{request.Id}' không tồn tại.");

        // Step 3: Gọi method nghiệp vụ của Entity để thay đổi mật khẩu
        //         Entity tự validate: hash phải ≥ 60 ký tự (BCrypt standard)
        user.ChangePassword(request.NewPasswordHash);

        // Step 4: Persist trạng thái Entity đã thay đổi vào cơ sở dữ liệu
        return await _userRepository.UpdateAsync(user, ct);
    }
}