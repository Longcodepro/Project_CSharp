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
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public UpdateProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng cập nhật profile người dùng theo thứ tự:
    /// lấy Entity → gọi method nghiệp vụ của Entity → persist → map sang DTO → trả về.
    /// </summary>
    /// <param name="request">Command chứa Id và các thông tin profile cần cập nhật.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><see cref="UserProfileDto"/> phản ánh trạng thái profile sau khi cập nhật.</returns>
    /// <exception cref="DomainException">Ném ra nếu User không tồn tại hoặc validation Entity thất bại.</exception>
    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        // Step 1: Lấy User Entity từ repository theo Id hệ thống
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        // Step 2: Kiểm tra sự tồn tại — ném exception nếu không tìm thấy
        if (user is null)
            throw new DomainException($"Người dùng với Id '{request.Id}' không tồn tại.");

        // Step 3: Gọi method nghiệp vụ của Entity để thay đổi trạng thái
        //         Entity tự thực hiện validation (độ dài, ký tự hợp lệ, v.v.)
        user.UpdateProfile(request.DisplayName, request.Bio, request.AvatarUrl);

        // Step 4: Persist trạng thái Entity đã thay đổi vào cơ sở dữ liệu
        var updated = await _userRepository.UpdateAsync(user, ct);
        if (!updated)
            throw new DomainException("Không thể cập nhật thông tin người dùng. Vui lòng thử lại.");

        // Step 5: Map Entity sang DTO — ẩn các trường nhạy cảm (Id, Email, PasswordHash)
        return new UserProfileDto
        {
            IdDisplay     = user.IdDisplay,
            DisplayName   = user.DisplayName,
            AvatarUrl     = user.AvatarUrl,
            Bio           = user.Bio,
            Role          = user.IsArtist ? "Artist" : "User",
            TotalFollowers = user.TotalFollowers,
            CreatedAt     = user.CreatedAt,
            IsActive      = user.IsActive
        };
    }
}
