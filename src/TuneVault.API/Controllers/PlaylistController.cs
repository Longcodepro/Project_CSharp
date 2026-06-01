using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class PlaylistController : BaseApiController
{
    [HttpGet]
    public IActionResult GetAll() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost]
    public IActionResult Create() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("{playlistId}/tracks")]
    public IActionResult AddTrack(string playlistId) => StatusCode(StatusCodes.Status501NotImplemented);
}
