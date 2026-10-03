using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Services;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("metrics")]
public sealed class MetricsController(MetricsService metricsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var body = await metricsService.RenderAsync(cancellationToken);
            return Content(body, "text/plain; version=0.0.4");
        }
        catch
        {
            return StatusCode(503);
        }
    }
}
