using MyShowBook.Api.Enums;

namespace MyShowBook.Api.Models;

public sealed record ReservationResult(
    ReservationOutcome Outcome,
    ReservationResponse? Reservation);
