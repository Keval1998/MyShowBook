using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Helpers;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("metrics")]
public sealed class MetricsController(MetricsHelper metricsHelper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var body = await metricsHelper.RenderAsync(cancellationToken);
            return Content(body, "text/plain; version=0.0.4");
        }
        catch
        {
            return StatusCode(503);
        }
    }
}
