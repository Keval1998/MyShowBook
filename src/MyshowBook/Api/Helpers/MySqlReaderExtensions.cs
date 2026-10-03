using MySqlConnector;

namespace MyShowBook.Api.Helpers;

public static class MySqlReaderExtensions
{
    public static Guid GetGuidFromString(this MySqlDataReader reader, int ordinal) =>
        Guid.Parse(reader.GetString(ordinal));
}
