using System;

namespace TuneVault.Application.Abstractions;

public interface ICurrentUserService
{
    string? UserId { get; } // Changed from Guid? to string? to match JWT 'sub' claim
    string? UserName { get; }
    string? Role { get; } // Changed to return all roles as a comma-separated string
    bool IsAuthenticated { get; }
}