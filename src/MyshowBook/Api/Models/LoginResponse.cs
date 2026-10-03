namespace MyShowBook.Api.Models;

public sealed record LoginResponse(
    string AccessToken,
    Guid UserGuid,
    string Role);
