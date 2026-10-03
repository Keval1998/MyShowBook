namespace MyShowBook.Api.Models;

public sealed record ReserveRequest(
    IReadOnlyList<string> Seats,
    string IdempotencyKey);
