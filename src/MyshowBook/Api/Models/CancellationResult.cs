using MyShowBook.Api.Enums;

namespace MyShowBook.Api.Models;

public sealed record CancellationResult(
    CancellationOutcome Outcome,
    ReservationResponse? Reservation);
