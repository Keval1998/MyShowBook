namespace MyShowBook.Api.Models;

public sealed record CreateShowRequest(
    string Name,
    IReadOnlyList<string> Seats,
    long PricePaise);

public sealed record SeatResponse(
    Guid SeatGuid,
    string SeatNumber,
    string Status);

public sealed record ShowResponse(
    Guid ShowGuid,
    string Name,
    long PricePaise,
    int PerUserLimit,
    int TotalSeats,
    int AvailableSeats,
    int HeldSeats,
    int ConfirmedSeats,
    IReadOnlyList<SeatResponse> Seats);
