// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetUserProfile/GetUserProfileQueryHandler.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetUserProfile;

/// <summary>
/// Bộ xử lý nghiệp vụ (Handler) chịu trách nhiệm tiếp nhận truy vấn lấy profile đầy đủ của người dùng theo Id.
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo một phiên bản xử lý mới của lớp <see cref="GetUserProfileQueryHandler"/> cùng với kho dữ liệu người dùng.
    /// </summary>
    /// <param name="userRepository">Giao diện kết nối kho dữ liệu thực thể User.</param>
    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Thực hiện xử lý logic nghiệp vụ đọc toàn bộ thông tin profile từ tầng Domain
    /// và ánh xạ sang UserProfileDto chỉ chứa các trường thông tin công khai.
    /// </summary>
    /// <param name="request">Đối tượng chứa tham số Id của người dùng cần lấy profile.</param>
    /// <param name="cancellationToken">Mã token điều phối hủy bỏ luồng nếu có sự cố ngắt kết nối.</param>
    /// <returns>Đối tượng <see cref="UserProfileDto"/> chứa thông tin profile công khai trả về phía Client.</returns>
    /// <exception cref="KeyNotFoundException">Ném ra ngoại lệ nếu mã Id gửi lên không khớp với bất kỳ bản ghi nào trong hệ thống.</exception>
    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User với Id '{request.Id}' không tồn tại.");

        return new UserProfileDto
        {
            IdDisplay = user.IdDisplay,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.IsArtist ? "Artist" : "User",
            TotalFollowers = user.TotalFollowers,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}