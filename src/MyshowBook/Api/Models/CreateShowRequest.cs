namespace MyShowBook.Api.Models;

public sealed record CreateShowRequest(
    string Name,
    IReadOnlyList<string> Seats,
    long PricePaise);
