using System.Data;
using MySqlConnector;

namespace MyShowBook.Api.Utility.Database;

public sealed class DatabaseUtility(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

    public async Task<T> ExecuteProcedureAsync<T>(
        string procedureName,
        IReadOnlyCollection<MySqlParameter> parameters,
        Func<MySqlDataReader, CancellationToken, Task<T>> readResult,
        Func<MySqlConnection, CancellationToken, Task>? prepareConnection = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (prepareConnection is not null)
            await prepareConnection(connection, cancellationToken);

        await using var command = CreateCommand(connection, procedureName, parameters);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await readResult(reader, cancellationToken);
    }

    public async Task<T> ExecuteProcedureWithTransientRetryAsync<T>(
        string procedureName,
        IReadOnlyCollection<MySqlParameter> parameters,
        Func<MySqlDataReader, CancellationToken, Task<T>> readResult,
        Func<MySqlConnection, CancellationToken, Task>? prepareConnection = null,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await ExecuteProcedureAsync(
                    procedureName,
                    parameters,
                    readResult,
                    prepareConnection,
                    cancellationToken);
            }
            catch (MySqlException ex) when (
                attempt < maxAttempts &&
                (ex.Number == 1205 || ex.Number == 1213))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static MySqlCommand CreateCommand(
        MySqlConnection connection,
        string procedureName,
        IReadOnlyCollection<MySqlParameter> parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 60;
        command.Parameters.AddRange(parameters.ToArray());
        return command;
    }
}
