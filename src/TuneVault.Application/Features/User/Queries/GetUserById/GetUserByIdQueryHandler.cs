using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetUserById;

/// <summary>
/// Handler xử lý <see cref="GetUserByIdQuery"/>.
/// Lấy <see cref="TuneVault.Domain.Entities.User"/> Entity và map sang <see cref="UserDto"/>
/// chứa thông tin cơ bản — không lộ Id hệ thống, Email hay PasswordHash.
/// Trả về <c>null</c> nếu không tìm thấy (soft-not-found — cho phép caller xử lý tùy ý).
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng lấy thông tin cơ bản người dùng theo Id:
    /// truy vấn Entity → kiểm tra null → map sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query chứa Id của người dùng cần tìm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// <see cref="UserDto"/> nếu tìm thấy; <c>null</c> nếu không tồn tại.
    /// </returns>
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        // Step 1: Truy vấn User Entity từ repository theo Id hệ thống
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        // Step 2: Trả về null nếu không tìm thấy (soft not-found)
        if (user is null)
            return null;

        // Step 3: Map Entity sang UserDto — ẩn Id hệ thống, Email, PasswordHash, Bio
        //         UserDto chỉ chứa thông tin đủ để hiển thị trong danh sách hoặc mention
        return new UserDto
        {
            IdDisplay   = user.IdDisplay,
            DisplayName = user.DisplayName,
            AvatarUrl   = user.AvatarUrl,
            Role        = user.IsArtist ? "Artist" : "User",
            IsActive    = user.IsActive
        };
    }
}
