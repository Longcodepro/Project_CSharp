// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetUserProfile/GetUserProfileQuery.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetUserProfile;

/// <summary>
/// Gói tin truy vấn (Query) đại diện cho yêu cầu lấy profile đầy đủ của người dùng theo Id hệ thống.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của người dùng cần lấy profile (ví dụ: U001).</param>
public record GetUserProfileQuery(string Id) : IRequest<UserProfileDto>;