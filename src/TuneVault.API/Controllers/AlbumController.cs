using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class AlbumController : BaseApiController
{
    [HttpGet]
    public IActionResult GetAll() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);
}
