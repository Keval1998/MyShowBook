using MySqlConnector;

namespace MyShowBook.Api.Utility.Database;

public sealed class TemporaryTableUtility
{
    public Task CreateSeatInputAsync(
        MySqlConnection connection,
        string tableName,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken) =>
        CreateAsync(connection, tableName, seatNumbers, cancellationToken);

    private static async Task CreateAsync(
        MySqlConnection connection,
        string tableName,
        IReadOnlyCollection<string> seatNumbers,
        CancellationToken cancellationToken)
    {
        if (tableName is not ("tmp_show_seats" or "tmp_reservation_seats"))
            throw new ArgumentException("Unexpected temporary table name.", nameof(tableName));

        await ExecuteAsync(
            connection,
            $"DROP TEMPORARY TABLE IF EXISTS {tableName};",
            cancellationToken);

        await ExecuteAsync(
            connection,
            $"CREATE TEMPORARY TABLE {tableName} (SeatNumber VARCHAR(32) NOT NULL PRIMARY KEY) ENGINE=InnoDB;",
            cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} (SeatNumber) VALUES (@seatNumber);";
        var parameter = command.Parameters.Add("@seatNumber", MySqlDbType.VarChar, 32);

        foreach (var seatNumber in seatNumbers)
        {
            parameter.Value = seatNumber;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task ExecuteAsync(
        MySqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
