using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Database;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(IDbConnectionFactory connectionFactory) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "ok" });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await connectionFactory.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
            return Ok(new { status = "ready" });
        }
        catch
        {
            return StatusCode(503, new { status = "not_ready" });
        }
    }
}
