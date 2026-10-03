using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShowBook.Api.Enums;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;
using MyShowBook.Api.Utility;

namespace MyShowBook.Api.Controllers;

[ApiController]
[Route("reservations")]
public sealed class ReservationsController(ReservationHelper reservationHelper) : ControllerBase
{
    [HttpPost("{reservationGuid:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<ReservationResponse>> Cancel(
        Guid reservationGuid,
        CancellationToken cancellationToken)
    {
        var userGuid = CurrentUserUtility.GetUserGuid(User);
        var result = await reservationHelper.CancelAsync(
            userGuid, reservationGuid, cancellationToken);

        return result.Outcome switch
        {
            CancellationOutcome.Cancelled => Ok(result.Reservation),
            CancellationOutcome.NotFound => NotFound(),
            CancellationOutcome.Conflict => Conflict(
                new { error = "Reservation cannot be cancelled in its current state." }),
            _ => StatusCode(500, new { error = "Unexpected cancellation result." })
        };
    }
}
