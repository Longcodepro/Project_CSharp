using MediatR;
using TuneVault.Application.Features.History.DTOs;

namespace TuneVault.Application.Features.History.Queries.GetHistoryResume;

/// <summary>
/// Query lấy thông tin phát tiếp của một media trong lịch sử người dùng.
/// </summary>
/// <param name="UserId">Mã người dùng hiện tại.</param>
/// <param name="MediaId">Mã media cần lấy trạng thái phát tiếp.</param>
public sealed record GetHistoryResumeQuery(string UserId, string MediaId) : IRequest<HistoryResumeDto?>;
