using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class ShareController : BaseApiController
{
    [HttpPost]
    public IActionResult Share() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("received")]
    public IActionResult GetSharedWithMe() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("sent")]
    public IActionResult GetSharedByMe() => StatusCode(StatusCodes.Status501NotImplemented);
}
