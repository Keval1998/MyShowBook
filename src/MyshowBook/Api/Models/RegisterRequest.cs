namespace MyShowBook.Api.Models;

public sealed record RegisterRequest(
    string Username,
    string Password,
    string Role);