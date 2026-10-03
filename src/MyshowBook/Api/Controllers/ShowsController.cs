using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;
using MyShowBook.Api.Services;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("shows")]
public sealed class ShowsController(
    ShowService showService,
    ReservationService reservationService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ShowResponse>> Create(
        CreateShowRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return StatusCode(
                StatusCodes.Status201Created,
                await showService.CreateAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{showGuid:guid}")]
    public async Task<ActionResult<ShowResponse>> Get(
        Guid showGuid,
        CancellationToken cancellationToken)
    {
        var response = await showService.GetAsync(showGuid, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("{showGuid:guid}/reserve")]
    [Authorize]
    public async Task<ActionResult<ReservationResponse>> Reserve(
        Guid showGuid,
        ReserveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = CurrentUserHelper.GetUserGuid(User);
            var result = await reservationService.ReserveAsync(
                userGuid, showGuid, request, cancellationToken);

            return result.Outcome switch
            {
                "CONFIRMED" => StatusCode(
                    StatusCodes.Status201Created, result.Reservation),
                "IDEMPOTENT_REPLAY" => Ok(result.Reservation),
                "SEAT_TAKEN" => Conflict(new { error = "One or more requested seats are already taken." }),
                "PER_USER_LIMIT" => Conflict(new { error = "Per-user booking limit exceeded." }),
                "IDEMPOTENCY_CONFLICT" => Conflict(new { error = "Idempotency key was already used with a different request." }),
                "INVALID_SEAT" => BadRequest(new { error = "One or more requested seats do not exist for this show." }),
                "SHOW_NOT_FOUND" => NotFound(),
                "USER_NOT_FOUND" => Unauthorized(),
                _ => StatusCode(500, new { error = "Unexpected reservation result." })
            };
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
