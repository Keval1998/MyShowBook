namespace MyShowBook.Api.Utility.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = "MyShowBook";
    public string Audience { get; init; } = "MyShowBook";
    public int ExpiryMinutes { get; init; } = 60;
}
