using System.Data;
using MySqlConnector;

namespace MyShowBook.Api.Database;

public static class StoredProcedureHelper
{
    public static MySqlCommand Create(
        MySqlConnection connection,
        string procedureName,
        params MySqlParameter[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 60;
        command.Parameters.AddRange(parameters);
        return command;
    }

    public static MySqlParameter StringParameter(string name, string value) =>
        new(name, MySqlDbType.VarChar) { Value = value };

    public static MySqlParameter LongParameter(string name, long value) =>
        new(name, MySqlDbType.Int64) { Value = value };

    public static MySqlParameter IntParameter(string name, int value) =>
        new(name, MySqlDbType.Int32) { Value = value };
}
