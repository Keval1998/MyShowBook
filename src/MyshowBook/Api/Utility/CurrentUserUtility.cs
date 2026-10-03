using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyShowBook.Api.Utility;

public static class CurrentUserUtility
{
    public static Guid GetUserGuid(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var userGuid)
            ? userGuid
            : throw new UnauthorizedAccessException("Authenticated user identity is missing.");
    }
}
