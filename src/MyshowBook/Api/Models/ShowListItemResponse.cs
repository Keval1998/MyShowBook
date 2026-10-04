namespace MyShowBook.Api.Models;

public sealed record ShowListItemResponse(
    Guid ShowGuid,
    string Name,
    long PricePaise,
    int TotalSeats,
    int AvailableSeats,
    int HeldSeats,
    int ConfirmedSeats);