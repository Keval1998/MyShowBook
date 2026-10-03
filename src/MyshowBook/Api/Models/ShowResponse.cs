namespace MyShowBook.Api.Models;

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
