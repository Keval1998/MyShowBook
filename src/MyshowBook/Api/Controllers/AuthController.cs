using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Models;
using MyShowBook.Api.Services;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<LoginResponse>> Token(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null ? Unauthorized() : Ok(response);
    }
}
