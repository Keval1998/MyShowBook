namespace MyShowBook.Api.Models;

public sealed record RegisterResponse(
    Guid UserGuid,
    string Username,
    string Role);