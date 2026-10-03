using MyShowBook.Api.Enums;

namespace MyShowBook.Api.Models;

public sealed record ReserveRequest(
    IReadOnlyList<string> Seats,
    string IdempotencyKey);

public sealed record ReservationResponse(
    Guid ReservationGuid,
    Guid ShowGuid,
    Guid UserGuid,
    IReadOnlyList<string> Seats,
    long AmountPaise,
    string Status);

public sealed record ReservationResult(
    ReservationOutcome Outcome,
    ReservationResponse? Reservation);
