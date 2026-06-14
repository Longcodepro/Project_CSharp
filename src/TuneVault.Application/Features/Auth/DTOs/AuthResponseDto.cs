using System.Collections.Generic;

namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record AuthResponseDto(string AccessToken, string UserId, string IdDisplay, IEnumerable<string> Roles);