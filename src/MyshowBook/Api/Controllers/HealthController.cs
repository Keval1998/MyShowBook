using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Utility.Database;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(DatabaseUtility database) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "ok" });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken) =>
        await database.CanConnectAsync(cancellationToken)
            ? Ok(new { status = "ready" })
            : StatusCode(503, new { status = "not_ready" });
}
