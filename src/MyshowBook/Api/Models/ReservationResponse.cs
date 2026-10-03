namespace MyShowBook.Api.Models;

public sealed record ReservationResponse(
    Guid ReservationGuid,
    Guid ShowGuid,
    Guid UserGuid,
    IReadOnlyList<string> Seats,
    long AmountPaise,
    string Status);
