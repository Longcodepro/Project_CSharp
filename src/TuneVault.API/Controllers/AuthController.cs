using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class AuthController : BaseApiController
{
    [HttpPost("register")]
    public IActionResult Register() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("login")]
    public IActionResult Login() => StatusCode(StatusCodes.Status501NotImplemented);
}
