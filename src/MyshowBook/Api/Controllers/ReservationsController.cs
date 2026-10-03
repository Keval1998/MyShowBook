using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;
using MyShowBook.Api.Services;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("reservations")]
public sealed class ReservationsController(ReservationService reservationService) : ControllerBase
{
    [HttpPost("{reservationGuid:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<ReservationResponse>> Cancel(
        Guid reservationGuid,
        CancellationToken cancellationToken)
    {
        var userGuid = CurrentUserHelper.GetUserGuid(User);
        var result = await reservationService.CancelAsync(
            userGuid, reservationGuid, cancellationToken);

        return result.Outcome switch
        {
            "CANCELLED" => Ok(result.Reservation),
            "NOT_FOUND" => NotFound(),
            "CANCEL_CONFLICT" => Conflict(new { error = "Reservation cannot be cancelled in its current state." }),
            _ => StatusCode(500, new { error = "Unexpected cancellation result." })
        };
    }
}
