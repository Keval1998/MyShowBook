using MySqlConnector;

namespace MyShowBook.Api.Utility;

public static class MySqlReaderUtility
{
    public static Guid GetGuid(this MySqlDataReader reader, int ordinal) =>
        Guid.Parse(reader.GetString(ordinal));
}
