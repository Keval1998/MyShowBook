using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("logs")]
[Authorize(Roles = "admin")]
public sealed class LogsController(IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var directory = Path.Combine(environment.ContentRootPath, "logs");
        var latestFile = Directory.Exists(directory)
            ? Directory.GetFiles(directory, "myshowbook-*.log")
                .OrderByDescending(Path.GetFileName)
                .FirstOrDefault()
            : null;

        if (latestFile is null)
            return NotFound(new { error = "No log file is available yet." });

        var lines = await System.IO.File.ReadAllLinesAsync(latestFile, cancellationToken);
        var content = string.Join(Environment.NewLine, lines.TakeLast(500));

        return Content(content, "application/x-ndjson");
    }
}