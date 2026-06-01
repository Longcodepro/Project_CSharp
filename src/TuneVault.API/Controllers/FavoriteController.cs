using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class FavoriteController : BaseApiController
{
    [HttpPost("toggle")]
    public IActionResult Toggle() => StatusCode(StatusCodes.Status501NotImplemented);
}
