using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.VerifyAsArtist;

/// <summary>
/// Handler xử lý <see cref="VerifyAsArtistCommand"/>.
/// Điều phối: kiểm tra quyền Admin → lấy Entity → gọi <c>VerifyAsArtist()</c> (Entity tự guard nếu đã là artist) → persist → map DTO.
/// Phân quyền: chỉ tài khoản có role "Admin" mới được thực hiện xác thực nghệ sĩ.
/// </summary>
public class VerifyAsArtistCommandHandler : IRequestHandler<VerifyAsArtistCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền Admin.</param>
    public VerifyAsArtistCommandHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng xác thực nghệ sĩ:
    /// kiểm tra xác thực & quyền Admin → lấy Entity → gọi <c>VerifyAsArtist()</c> → persist → map sang DTO.
    /// </summary>
    /// <param name="request">Command chứa Id của người dùng cần xác thực là nghệ sĩ.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><see cref="UserProfileDto"/> phản ánh trạng thái sau khi xác thực (Role = "Artist").</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu người dùng hiện tại không có role "Admin".</exception>
    /// <exception cref="DomainException">
    /// Ném ra nếu User không tồn tại hoặc đã là Artist rồi (thrown bởi Entity).
    /// </exception>
    public async Task<UserProfileDto> Handle(VerifyAsArtistCommand request, CancellationToken ct)
    {
        // Step 0: Kiểm tra đã xác thực chưa
        if (string.IsNullOrWhiteSpace(_currentUserContext.GetCurrentUserId()))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi thực hiện xác thực nghệ sĩ.");

        // Step 0.1: Chỉ Admin mới được phép xác thực tài khoản nghệ sĩ
        if (!_currentUserContext.HasRole("Admin"))
            throw new ForbiddenAccessException("Chỉ Admin mới có quyền xác thực tài khoản nghệ sĩ.");

        // Step 1: Lấy User Entity từ repository theo Id hệ thống
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        // Step 2: Kiểm tra sự tồn tại — ném exception nếu không tìm thấy
        if (user is null)
            throw new DomainException($"Người dùng với Id '{request.Id}' không tồn tại.");

        // Step 3: Gọi method nghiệp vụ của Entity
        //         Entity tự ném DomainException nếu user.IsArtist đã là true
        user.VerifyAsArtist();

        // Step 4: Persist trạng thái Entity đã thay đổi (IsArtist = true) vào DB
        var updated = await _userRepository.UpdateAsync(user, ct);
        if (!updated)
            throw new DomainException("Không thể cập nhật trạng thái nghệ sĩ. Vui lòng thử lại.");

        // Step 5: Map Entity sang DTO — ẩn các trường nhạy cảm
        return new UserProfileDto
        {
            IdDisplay      = user.IdDisplay,
            DisplayName    = user.DisplayName,
            AvatarUrl      = user.AvatarUrl,
            Bio            = user.Bio,
            Role           = "Artist",
            TotalFollowers = user.TotalFollowers,
            FollowingCount = (await _userRepository.GetFollowingAsync(user.Id, ct)).Count(),
            CreatedAt      = user.CreatedAt,
            IsActive       = user.IsActive
        };
    }
}
