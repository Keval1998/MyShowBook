namespace MyShowBook.Api.Models;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    string AccessToken,
    Guid UserGuid,
    string Role);
