using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class HistoryController : BaseApiController
{
    [HttpGet("recent")]
    public IActionResult GetRecent() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Record() => StatusCode(StatusCodes.Status501NotImplemented);
}
