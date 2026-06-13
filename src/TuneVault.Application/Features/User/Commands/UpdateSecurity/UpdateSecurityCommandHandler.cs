using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.UpdateSecurity;

/// <summary>
/// Handler xử lý <see cref="UpdateSecurityCommand"/>.
/// Điều phối luồng thay đổi mật khẩu: lấy Entity → gọi <c>ChangePassword</c> → persist.
/// Không trả về DTO vì đây là thao tác bảo mật — không cần expose dữ liệu nhạy cảm.
/// </summary>
public class UpdateSecurityCommandHandler : IRequestHandler<UpdateSecurityCommand, bool>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public UpdateSecurityCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng cập nhật mật khẩu:
    /// lấy Entity → gọi method <c>ChangePassword</c> của Entity → persist.
    /// </summary>
    /// <param name="request">Command chứa Id và PasswordHash mới đã được mã hóa.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu cập nhật thành công.</returns>
    /// <exception cref="DomainException">Ném ra nếu User không tồn tại hoặc hash không hợp lệ.</exception>
    public async Task<bool> Handle(UpdateSecurityCommand request, CancellationToken ct)
    {
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
