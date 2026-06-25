using FluentValidation;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

public sealed class ShareMediaCommandValidator : AbstractValidator<ShareMediaCommand>
{
    public ShareMediaCommandValidator()
    {
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("Không xác định được người gửi từ token đăng nhập.");
        RuleFor(x => x.ReceiverId).NotEmpty().WithMessage("Mã người nhận chia sẻ không được để trống.");
        RuleFor(x => x.SharedItemId).NotEmpty().WithMessage("Mã nội dung cần chia sẻ không được để trống.");
        RuleFor(x => x.ShareType)
            .NotEmpty()
            .Must(IsValidShareType)
            .WithMessage("Loại nội dung chia sẻ phải là Track, Album, Playlist, Media, Song hoặc Video.");
        RuleFor(x => x)
            .Must(x => x.SenderId != x.ReceiverId)
            .WithMessage("Không thể tự chia sẻ cho chính mình.");
    }

    private static bool IsValidShareType(string shareType)
    {
        return shareType.Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant() switch
            {
                "playlist" or "album" or "track" or "media" or "mediaitem" or "song" or "video" => true,
                _ => false
            };
    }
}
