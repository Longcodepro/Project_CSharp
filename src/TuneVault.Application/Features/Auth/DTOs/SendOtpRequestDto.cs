namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record SendOtpRequestDto(string Email, string Purpose); // "register" | "reset_password"