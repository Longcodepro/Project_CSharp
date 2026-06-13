using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.VerifyAsArtist;

/// <summary>
/// Handler xử lý <see cref="VerifyAsArtistCommand"/>.
/// Điều phối: lấy Entity → gọi <c>VerifyAsArtist()</c> (Entity tự guard nếu đã là artist) → persist → map DTO.
/// </summary>
public class VerifyAsArtistCommandHandler : IRequestHandler<VerifyAsArtistCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public VerifyAsArtistCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng xác thực nghệ sĩ:
    /// lấy Entity → gọi <c>VerifyAsArtist()</c> → persist → map sang DTO.
    /// </summary>
    /// <param name="request">Command chứa Id của người dùng cần xác thực là nghệ sĩ.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><see cref="UserProfileDto"/> phản ánh trạng thái sau khi xác thực (Role = "Artist").</returns>
    /// <exception cref="DomainException">
    /// Ném ra nếu User không tồn tại hoặc đã là Artist rồi (thrown bởi Entity).
    /// </exception>
    public async Task<UserProfileDto> Handle(VerifyAsArtistCommand request, CancellationToken ct)
    {
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
            CreatedAt      = user.CreatedAt,
            IsActive       = user.IsActive
        };
    }
}
