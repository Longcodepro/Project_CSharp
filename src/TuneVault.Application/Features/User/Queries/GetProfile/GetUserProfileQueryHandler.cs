using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetProfile;

/// <summary>
/// Handler xử lý <see cref="GetUserProfileQuery"/>.
/// Lấy <see cref="TuneVault.Domain.Entities.User"/> Entity từ repository và map sang
/// <see cref="UserProfileDto"/> để trả về thông tin profile đầy đủ mà không lộ dữ liệu nhạy cảm.
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng lấy profile người dùng:
    /// truy vấn Entity từ repository → kiểm tra tồn tại → map sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query chứa Id của người dùng cần lấy profile.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><see cref="UserProfileDto"/> chứa toàn bộ thông tin profile công khai.</returns>
    /// <exception cref="DomainException">Ném ra nếu không tìm thấy User với Id tương ứng.</exception>
    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        // Step 1: Truy vấn User Entity từ repository theo Id hệ thống
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        // Step 2: Kiểm tra sự tồn tại — ném exception nếu không tìm thấy
        if (user is null)
            throw new DomainException($"Người dùng với Id '{request.Id}' không tồn tại.");

        // Step 3: Map Entity sang UserProfileDto — ẩn Id hệ thống, Email, PasswordHash
        return new UserProfileDto
        {
            IdDisplay      = user.IdDisplay,
            DisplayName    = user.DisplayName,
            AvatarUrl      = user.AvatarUrl,
            Bio            = user.Bio,
            Role           = user.IsArtist ? "Artist" : "User",
            TotalFollowers = user.TotalFollowers,
            CreatedAt      = user.CreatedAt,
            IsActive       = user.IsActive
        };
    }
}
