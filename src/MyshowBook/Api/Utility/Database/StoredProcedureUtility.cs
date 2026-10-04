using MySqlConnector;

namespace MyShowBook.Api.Utility.Database;

public static class StoredProcedureUtility
{
    public static MySqlParameter String(string name, string value) =>
        new(name, MySqlDbType.VarChar) { Value = value };

    public static MySqlParameter Long(string name, long value) =>
        new(name, MySqlDbType.Int64) { Value = value };

    public static MySqlParameter Int(string name, int value) =>
        new(name, MySqlDbType.Int32) { Value = value };

    public static MySqlParameter Bool(string name, bool value) =>
        new(name, MySqlDbType.Boolean) { Value = value };
}
