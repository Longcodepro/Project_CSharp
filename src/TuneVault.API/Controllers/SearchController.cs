using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class SearchController : BaseApiController
{
    [HttpGet]
    public IActionResult Search([FromQuery] string keyword) => StatusCode(StatusCodes.Status501NotImplemented);
}
