using MyShowBook.Api.Constants;
using MyShowBook.Api.Models;
using MyShowBook.Api.Utility;
using MyShowBook.Api.Utility.Database;

namespace MyShowBook.Api.Helpers;

public sealed class RegistrationHelper(DatabaseUtility database, ILogger<RegistrationHelper> logger)
{
    public async Task<RegisterResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        var username = request.Username.Trim();
        var role = request.Role.Trim().ToLowerInvariant();
        var userGuid = Guid.NewGuid();

        try
        {
            return await database.ExecuteProcedureAsync(
                StoredProcedureNames.RegisterUser,
                [
                    StoredProcedureUtility.String("p_user_guid", userGuid.ToString("D")),
                    StoredProcedureUtility.String("p_username", username),
                    StoredProcedureUtility.String("p_password_hash", PasswordHasher.Hash(request.Password)),
                    StoredProcedureUtility.Bool("p_is_admin", role == "admin")
                ],
                async (reader, ct) =>
                {
                    if (!await reader.ReadAsync(ct))
                        return null;

                    return new RegisterResponse(
                        Guid.Parse(reader.GetString(0)),
                        reader.GetString(1),
                        reader.GetBoolean(2) ? "admin" : "customer");
                },
                cancellationToken: cancellationToken);
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            logger.LogWarning(ex, "Registration rejected because username already exists: {Username}", username);
            return null;
        }
    }

    private static void Validate(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            request.Username.Trim().Length > 64)
            throw new ArgumentException("Username is required and must be at most 64 characters.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.");

        var role = request.Role?.Trim().ToLowerInvariant();
        if (role is not ("admin" or "customer"))
            throw new ArgumentException("Role must be admin or customer.");
    }
}