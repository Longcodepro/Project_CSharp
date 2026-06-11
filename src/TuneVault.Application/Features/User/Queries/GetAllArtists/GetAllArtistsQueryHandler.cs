// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetAllArtists/GetAllArtistsQueryHandler.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetAllArtists;

/// <summary>
/// Bộ xử lý nghiệp vụ (Handler) chịu trách nhiệm tiếp nhận truy vấn lấy toàn bộ
/// danh sách người dùng đã được xác thực là nghệ sĩ trong hệ thống.
/// </summary>
public class GetAllArtistsQueryHandler : IRequestHandler<GetAllArtistsQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo một phiên bản xử lý mới của lớp <see cref="GetAllArtistsQueryHandler"/> cùng với kho dữ liệu người dùng.
    /// </summary>
    /// <param name="userRepository">Giao diện kết nối kho dữ liệu thực thể User.</param>
    public GetAllArtistsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Thực hiện xử lý logic nghiệp vụ lấy danh sách nghệ sĩ từ tầng Domain
    /// và ánh xạ sang danh sách UserDto chỉ chứa các trường thông tin công khai.
    /// </summary>
    /// <param name="request">Đối tượng truy vấn không có tham số đầu vào.</param>
    /// <param name="cancellationToken">Mã token điều phối hủy bỏ luồng nếu có sự cố ngắt kết nối.</param>
    /// <returns>Danh sách các đối tượng <see cref="UserDto"/> đại diện cho các nghệ sĩ đang hoạt động.</returns>
    public async Task<IEnumerable<UserDto>> Handle(GetAllArtistsQuery request, CancellationToken cancellationToken)
    {
        var artists = await _userRepository.GetAllArtistsAsync(cancellationToken);

        return artists.Select(user => new UserDto
        {
            IdDisplay = user.IdDisplay,
            DisplayName = user.DisplayName,
            Role = "Artist",
            IsActive = user.IsActive
        });
    }
}