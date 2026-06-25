using MediatR;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

/// <summary>
/// Command tạo bản ghi chia sẻ và notification cho người nhận.
/// </summary>
/// <param name="SenderId">Mã người gửi.</param>
/// <param name="ReceiverId">Mã người nhận.</param>
/// <param name="ShareType">Loại item được chia sẻ.</param>
/// <param name="SharedItemId">Mã item được chia sẻ.</param>
/// <param name="Message">Lời nhắn của người gửi.</param>
public sealed record ShareMediaCommand(
    string SenderId,
    string ReceiverId,
    string ShareType,
    string SharedItemId,
    string? Message) : IRequest<string>;
