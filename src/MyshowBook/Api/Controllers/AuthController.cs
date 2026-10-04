using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    AuthHelper authHelper,
    RegistrationHelper registrationHelper,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<LoginResponse>> Token(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authHelper.LoginAsync(request, cancellationToken);

        if (response is null)
        {
            logger.LogWarning("Login failed for username {Username}", request.Username);
            return Unauthorized();
        }

        logger.LogInformation("Login succeeded for username {Username}", request.Username);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await registrationHelper.RegisterAsync(request, cancellationToken);

            return response is null
                ? Conflict(new { error = "Username already exists." })
                : StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}