using MyShowBook.Api.Authentication;
using MyShowBook.Api.Constants;
using MyShowBook.Api.Database;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;

namespace MyShowBook.Api.Services;

public sealed class AuthService(
    IDbConnectionFactory connectionFactory,
    JwtTokenService tokenService)
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await using var command = StoredProcedureHelper.Create(
            connection,
            StoredProcedureNames.GetUserForLogin,
            StoredProcedureHelper.StringParameter("p_username", request.Username));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var userGuid = reader.GetGuidFromString(0);
        var username = reader.GetString(1);
        var passwordHash = reader.GetString(2);
        var isAdmin = reader.GetBoolean(3);

        if (!PasswordHasher.Verify(request.Password, passwordHash))
            return null;

        return new LoginResponse(
            tokenService.Create(userGuid, username, isAdmin),
            userGuid,
            isAdmin ? "admin" : "user");
    }
}
