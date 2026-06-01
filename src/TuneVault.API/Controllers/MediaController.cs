using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class MediaController : BaseApiController
{
    [HttpGet]
    public IActionResult GetAll() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{id}")]
    public IActionResult GetById(string id) => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("upload")]
    public IActionResult Upload() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("{id}/stream")]
    public IActionResult Stream(string id) => StatusCode(StatusCodes.Status501NotImplemented);
}
