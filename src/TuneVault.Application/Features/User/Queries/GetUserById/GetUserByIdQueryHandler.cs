// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetUserById/GetUserByIdQueryHandler.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetUserById;

/// <summary>
/// Bộ xử lý nghiệp vụ (Handler) chịu trách nhiệm tiếp nhận truy vấn lấy thông tin người dùng cụ thể dựa trên Id.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo một phiên bản xử lý mới của lớp <see cref="GetUserByIdQueryHandler"/> cùng với kho dữ liệu người dùng.
    /// </summary>
    /// <param name="userRepository">Giao diện kết nối kho dữ liệu thực thể User.</param>
    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Thực hiện xử lý logic nghiệp vụ đọc dữ liệu từ tầng Domain và ánh xạ kết quả sang UserDto công khai.
    /// </summary>
    /// <param name="request">Đối tượng chứa tham số Id của người dùng cần tìm kiếm.</param>
    /// <param name="cancellationToken">Mã token điều phối hủy bỏ luồng nếu có sự cố ngắt kết nối.</param>
    /// <returns>Đối tượng <see cref="UserDto"/> chứa thông tin công khai trả về phía Client.</returns>
    /// <exception cref="KeyNotFoundException">Ném ra ngoại lệ nếu mã Id gửi lên không khớp với bất kỳ bản ghi nào trong hệ thống.</exception>
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User với Id '{request.Id}' không tồn tại.");

        return new UserDto
        {
            IdDisplay = user.IdDisplay,
            DisplayName = user.DisplayName,
            Role = user.IsArtist ? "Artist" : "User",
            IsActive = user.IsActive
        };
    }
}