using MyShowBook.Api.Constants;
using MyShowBook.Api.Models;
using MyShowBook.Api.Utility;
using MyShowBook.Api.Utility.Authentication;
using MyShowBook.Api.Utility.Database;

namespace MyShowBook.Api.Helpers;

public sealed class AuthHelper(
    DatabaseUtility database,
    JwtTokenService tokenService)
{
    public Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken) =>
        database.ExecuteProcedureAsync(
            StoredProcedureNames.GetUserForLogin,
            [StoredProcedureUtility.String("p_username", request.Username)],
            async (reader, ct) =>
            {
                if (!await reader.ReadAsync(ct))
                    return null;

                var userGuid = reader.GetGuid(0);
                var username = reader.GetString(1);
                var passwordHash = reader.GetString(2);
                var isAdmin = reader.GetBoolean(3);

                if (!PasswordHasher.Verify(request.Password, passwordHash))
                    return null;

                return new LoginResponse(
                    tokenService.Create(userGuid, username, isAdmin),
                    userGuid,
                    isAdmin ? "admin" : "user");
            },
            cancellationToken: cancellationToken);
}
