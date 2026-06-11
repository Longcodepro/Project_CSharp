using MediatR;

namespace TuneVault.Application.Features.Auth.Commands.Login;

// Request nhận vào IdDisplay (Username) và Password từ người dùng, kỳ vọng trả về chuỗi JWT Token (string)
public record LoginCommand(string IdDisplay, string Password) : IRequest<string>;