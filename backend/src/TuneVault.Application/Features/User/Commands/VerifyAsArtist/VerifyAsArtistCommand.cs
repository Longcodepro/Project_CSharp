using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Commands.VerifyAsArtist;

/// <summary>
/// Command (yêu cầu ghi) đại diện cho nghiệp vụ xác thực một tài khoản người dùng là nghệ sĩ.
/// Handler sẽ gọi <c>VerifyAsArtist()</c> trên Entity (Entity tự kiểm tra nếu đã là nghệ sĩ rồi),
/// sau đó persist và trả về DTO phản ánh trạng thái mới.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của tài khoản cần xác thực là nghệ sĩ (ví dụ: U001).</param>
public record VerifyAsArtistCommand(string Id) : IRequest<UserProfileDto>;
