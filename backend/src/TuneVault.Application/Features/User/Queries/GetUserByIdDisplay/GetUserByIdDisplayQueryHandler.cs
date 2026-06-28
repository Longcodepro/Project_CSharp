using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;

/// <summary>
/// Handler xử lý <see cref="GetUserByIdDisplayQuery"/>.
/// Tìm kiếm <see cref="TuneVault.Domain.Entities.User"/> Entity theo IdDisplay
/// và map sang <see cref="UserDto"/> — không lộ Id hệ thống, Email hay PasswordHash.
/// </summary>
public class GetUserByIdDisplayQueryHandler : IRequestHandler<GetUserByIdDisplayQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public GetUserByIdDisplayQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng tìm kiếm người dùng theo handle công khai:
    /// chuẩn hóa handle → truy vấn Entity → kiểm tra null → map sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query chứa IdDisplay cần tìm kiếm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// <see cref="UserDto"/> nếu tìm thấy; <c>null</c> nếu không tồn tại.
    /// </returns>
    public async Task<UserDto?> Handle(GetUserByIdDisplayQuery request, CancellationToken ct)
    {
        var normalizedHandle = request.IdDisplay.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByIdDisplayAsync(normalizedHandle, ct);

        if (user is null)
            return null;

        return new UserDto
        {
            Id          = user.Id,
            IdDisplay   = user.IdDisplay,
            DisplayName = user.DisplayName,
            AvatarUrl   = user.AvatarUrl,
            Role        = user.IsArtist ? "Artist" : "User",
            IsActive    = user.IsActive
        };
    }
}
