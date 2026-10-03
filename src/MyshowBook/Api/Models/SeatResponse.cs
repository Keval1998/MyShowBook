namespace MyShowBook.Api.Models;

public sealed record SeatResponse(
    Guid SeatGuid,
    string SeatNumber,
    string Status);
