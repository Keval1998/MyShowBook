using MySqlConnector;

namespace MyShowBook.Api.Utility.Database;

public sealed class DatabaseBootstrapper(
    IConfiguration configuration,
    ILogger<DatabaseBootstrapper> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("DatabaseBootstrap:Enabled"))
            return;

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

        var root = configuration["DatabaseBootstrap:Path"]
            ?? Path.Combine(AppContext.BaseDirectory, "database");

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await EnsureMarkerTableAsync(connection, cancellationToken);

        if (await IsBootstrapCompleteAsync(connection, cancellationToken))
        {
            logger.LogInformation("Database bootstrap already completed.");
            return;
        }

        var schemaExists = await TableExistsAsync(connection, "Users", cancellationToken);

        if (!schemaExists)
        {
            await ExecuteSqlFileStatementsAsync(
                connection,
                Path.Combine(root, "init", "001-create-tables.sql"),
                cancellationToken);

            foreach (var file in Directory.GetFiles(Path.Combine(root, "seed"), "*.sql").OrderBy(x => x))
                await ExecuteSqlFileStatementsAsync(connection, file, cancellationToken);
        }
        else
        {
            logger.LogInformation("Existing schema detected; skipping init and seed files.");
        }

        foreach (var file in Directory.GetFiles(Path.Combine(root, "functions"), "*.sql").OrderBy(x => x))
            await ExecuteProcedureFileAsync(connection, file, cancellationToken);

        await MarkBootstrapCompleteAsync(connection, cancellationToken);
        logger.LogInformation("Database bootstrap completed successfully.");
    }

    private static async Task EnsureMarkerTableAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS _MyShowBookBootstrap (
                Id TINYINT NOT NULL PRIMARY KEY,
                CompletedAtUtc DATETIME(6) NOT NULL
            ) ENGINE=InnoDB;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> IsBootstrapCompleteAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM _MyShowBookBootstrap WHERE Id = 1;";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private static async Task<bool> TableExistsAsync(
        MySqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND table_name = @table_name;
            """;
        command.Parameters.AddWithValue("@table_name", tableName);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private static async Task ExecuteSqlFileStatementsAsync(
        MySqlConnection connection,
        string path,
        CancellationToken cancellationToken)
    {
        var sql = await File.ReadAllTextAsync(path, cancellationToken);

        foreach (var statement in sql
                     .Split(';', StringSplitOptions.RemoveEmptyEntries)
                     .Select(x => x.Trim())
                     .Where(x => x.Length > 0))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task ExecuteProcedureFileAsync(
        MySqlConnection connection,
        string path,
        CancellationToken cancellationToken)
    {
        var sql = await File.ReadAllTextAsync(path, cancellationToken);

        sql = sql.Replace("DELIMITER $$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("DELIMITER //", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("DELIMITER ;", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("END$$", "END;", StringComparison.Ordinal)
            .Replace("END//", "END;", StringComparison.Ordinal);

        foreach (var statement in sql
                     .Split(new[] { "END;" }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(x => x.Trim())
                     .Where(x => x.Contains("CREATE PROCEDURE", StringComparison.OrdinalIgnoreCase)))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement + "END;";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task MarkBootstrapCompleteAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO _MyShowBookBootstrap (Id, CompletedAtUtc)
            VALUES (1, UTC_TIMESTAMP(6));
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
