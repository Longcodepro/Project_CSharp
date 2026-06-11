// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetUserByIdDisplay/GetUserByIdDisplayQueryHandler.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;

/// <summary>
/// Bộ xử lý nghiệp vụ (Handler) chịu trách nhiệm tiếp nhận truy vấn tìm kiếm người dùng theo IdDisplay.
/// </summary>
public class GetUserByIdDisplayQueryHandler : IRequestHandler<GetUserByIdDisplayQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo một phiên bản xử lý mới của lớp <see cref="GetUserByIdDisplayQueryHandler"/> cùng với kho dữ liệu người dùng.
    /// </summary>
    /// <param name="userRepository">Giao diện kết nối kho dữ liệu thực thể User.</param>
    public GetUserByIdDisplayQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Thực hiện xử lý logic nghiệp vụ tìm kiếm người dùng theo IdDisplay và ánh xạ kết quả sang UserDto công khai.
    /// </summary>
    /// <param name="request">Đối tượng chứa tham số IdDisplay của người dùng cần tìm kiếm.</param>
    /// <param name="cancellationToken">Mã token điều phối hủy bỏ luồng nếu có sự cố ngắt kết nối.</param>
    /// <returns>Đối tượng <see cref="UserDto"/> chứa thông tin công khai trả về phía Client.</returns>
    /// <exception cref="KeyNotFoundException">Ném ra ngoại lệ nếu IdDisplay gửi lên không khớp với bất kỳ bản ghi nào.</exception>
    public async Task<UserDto> Handle(GetUserByIdDisplayQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdDisplayAsync(request.IdDisplay, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với IdDisplay '{request.IdDisplay}'.");

        return new UserDto
        {
            IdDisplay = user.IdDisplay,
            DisplayName = user.DisplayName,
            Role = user.IsArtist ? "Artist" : "User",
            IsActive = user.IsActive
        };
    }
}