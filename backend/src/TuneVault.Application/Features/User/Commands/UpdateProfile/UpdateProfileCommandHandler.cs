using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.UpdateProfile;

/// <summary>
/// Handler xử lý <see cref="UpdateProfileCommand"/>.
/// Tuân thủ nguyên tắc Clean Architecture: tầng Application điều phối luồng,
/// logic nghiệp vụ (validation, mutation) thuộc về <see cref="TuneVault.Domain.Entities.User"/> Entity,
/// và mapping sang DTO xảy ra tại đây trước khi trả về Controller.
/// Phân quyền: chỉ người dùng đã đăng nhập và đang cập nhật hồ sơ của chính mình.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public UpdateProfileCommandHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng cập nhật profile người dùng theo thứ tự:
    /// kiểm tra xác thực & quyền sở hữu → lấy Entity → gọi method nghiệp vụ của Entity → persist → map sang DTO → trả về.
    /// </summary>
    /// <param name="request">Command chứa Id và các thông tin profile cần cập nhật.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><see cref="UserProfileDto"/> phản ánh trạng thái profile sau khi cập nhật.</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu Id khác với người dùng hiện tại.</exception>
    /// <exception cref="DomainException">Ném ra nếu User không tồn tại hoặc validation Entity thất bại.</exception>
    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi cập nhật hồ sơ.");

        if (!currentUserId.Equals(request.Id, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền cập nhật hồ sơ của người dùng khác.");

        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        if (user is null)
            throw new DomainException($"Người dùng với Id '{request.Id}' không tồn tại.");

        var nextIdDisplay = request.IdDisplay.Trim().ToLowerInvariant();

        if (!string.Equals(user.IdDisplay, nextIdDisplay, StringComparison.OrdinalIgnoreCase))
        {
            var existingByHandle = await _userRepository.GetByIdDisplayAsync(nextIdDisplay, ct);
            if (existingByHandle is not null && !string.Equals(existingByHandle.Id, user.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException($"Tên người dùng '{nextIdDisplay}' đã tồn tại.");
            }
        }

        //         Entity tự thực hiện validation (độ dài, ký tự hợp lệ, v.v.)
        var avatarUrl = request.ShouldUpdateAvatar
            ? request.AvatarUrl
            : user.AvatarUrl;

        user.UpdateProfile(nextIdDisplay, request.DisplayName, request.Bio, avatarUrl);

        var updated = await _userRepository.UpdateAsync(user, ct);
        if (!updated)
            throw new DomainException("Không thể cập nhật thông tin người dùng. Vui lòng thử lại.");

        var followerCount = (await _userRepository.GetFollowersAsync(user.Id, ct)).Count();
        var followingCount = (await _userRepository.GetFollowingAsync(user.Id, ct)).Count();

        return new UserProfileDto
        {
            IdDisplay      = user.IdDisplay,
            DisplayName    = user.DisplayName,
            Email          = user.Email,
            AvatarUrl      = user.AvatarUrl,
            Bio            = user.Bio,
            Role           = user.IsArtist ? "Artist" : "User",
            TotalFollowers = followerCount,
            FollowingCount = followingCount,
            CreatedAt      = user.CreatedAt,
            IsActive       = user.IsActive
        };
    }
}
