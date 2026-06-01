using Microsoft.AspNetCore.Mvc;

namespace TuneVault.API.Controllers;

public sealed class FollowController : BaseApiController
{
    [HttpPost]
    public IActionResult Follow() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpDelete]
    public IActionResult Unfollow() => StatusCode(StatusCodes.Status501NotImplemented);
}
