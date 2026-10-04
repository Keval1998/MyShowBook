using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Enums;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;
using MyShowBook.Api.Utility;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("shows")]
public sealed class ShowsController(
    ShowHelper showHelper,
    ReservationHelper reservationHelper) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ShowResponse>> Create(
        CreateShowRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await showHelper.CreateAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShowListItemResponse>>> List(
        CancellationToken cancellationToken) =>
        Ok(await showHelper.ListAsync(cancellationToken));

    [HttpGet("{showGuid:guid}")]
    public async Task<ActionResult<ShowResponse>> Get(
        Guid showGuid,
        CancellationToken cancellationToken)
    {
        var response = await showHelper.GetAsync(showGuid, cancellationToken);
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
            var userGuid = CurrentUserUtility.GetUserGuid(User);
            var result = await reservationHelper.ReserveAsync(
                userGuid, showGuid, request, cancellationToken);

            return result.Outcome switch
            {
                ReservationOutcome.Confirmed => StatusCode(
                    StatusCodes.Status201Created, result.Reservation),
                ReservationOutcome.IdempotentReplay => Ok(result.Reservation),
                ReservationOutcome.SeatTaken => Conflict(
                    new { error = "One or more requested seats are already taken." }),
                ReservationOutcome.PerUserLimit => Conflict(
                    new { error = "Per-user booking limit exceeded." }),
                ReservationOutcome.IdempotencyConflict => Conflict(
                    new { error = "Idempotency key was already used with a different request." }),
                ReservationOutcome.InvalidSeat => BadRequest(
                    new { error = "One or more requested seats do not exist for this show." }),
                ReservationOutcome.ShowNotFound => NotFound(),
                ReservationOutcome.UserNotFound => Unauthorized(),
                _ => StatusCode(500, new { error = "Unexpected reservation result." })
            };
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}