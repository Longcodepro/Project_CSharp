namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record ResetPasswordRequestDto(string Email, string OtpCode, string NewPassword); // NewPassword: thô