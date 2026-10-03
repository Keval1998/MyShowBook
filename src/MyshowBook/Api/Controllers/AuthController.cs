using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(AuthHelper authHelper) : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<LoginResponse>> Token(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authHelper.LoginAsync(request, cancellationToken);
        return response is null ? Unauthorized() : Ok(response);
    }
}
