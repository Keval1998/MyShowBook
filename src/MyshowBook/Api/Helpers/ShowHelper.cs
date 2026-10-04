using MyShowBook.Api.Constants;
using MyShowBook.Api.Models;
using MyShowBook.Api.Utility;
using MyShowBook.Api.Utility.Database;

namespace MyShowBook.Api.Helpers;

public sealed class ShowHelper(
    DatabaseUtility database,
    TemporaryTableUtility temporaryTables)
{
    public Task<ShowResponse> CreateAsync(
        CreateShowRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCreateRequest(request);
        var seats = NormalizeSeats(request.Seats);

        return database.ExecuteProcedureAsync(
            StoredProcedureNames.CreateShow,
            [
                StoredProcedureUtility.String("p_show_guid", Guid.NewGuid().ToString("D")),
                StoredProcedureUtility.String("p_name", request.Name.Trim()),
                StoredProcedureUtility.Long("p_price_paise", request.PricePaise),
                StoredProcedureUtility.Int("p_per_user_limit", 4)
            ],
            ReadShowAsync,
            (connection, ct) => temporaryTables.CreateSeatInputAsync(
                connection, TempTableNames.ShowSeats, seats, ct),
            cancellationToken);
    }

    public Task<IReadOnlyList<ShowListItemResponse>> ListAsync(
        CancellationToken cancellationToken) =>
        database.ExecuteProcedureAsync(
            StoredProcedureNames.ListShows,
            [],
            ReadShowListAsync,
            cancellationToken: cancellationToken);

    public Task<ShowResponse?> GetAsync(
        Guid showGuid,
        CancellationToken cancellationToken) =>
        database.ExecuteProcedureAsync(
            StoredProcedureNames.GetShow,
            [StoredProcedureUtility.String("p_show_guid", showGuid.ToString("D"))],
            ReadExistingShowAsync,
            cancellationToken: cancellationToken);

    private static async Task<IReadOnlyList<ShowListItemResponse>> ReadShowListAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        var shows = new List<ShowListItemResponse>();

        while (await reader.ReadAsync(cancellationToken))
        {
            shows.Add(new ShowListItemResponse(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetInt64(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return shows;
    }

    private static async Task<ShowResponse> ReadShowAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Show procedure returned no show.");

        var response = ReadShowSummary(reader);

        if (!await reader.NextResultAsync(cancellationToken))
            return response;

        return response with { Seats = await ReadSeatsAsync(reader, cancellationToken) };
    }

    private static async Task<ShowResponse?> ReadExistingShowAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var response = ReadShowSummary(reader);

        if (!await reader.NextResultAsync(cancellationToken))
            return response;

        return response with { Seats = await ReadSeatsAsync(reader, cancellationToken) };
    }

    private static ShowResponse ReadShowSummary(MySqlConnector.MySqlDataReader reader) =>
        new(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetInt64(2),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            reader.GetInt32(6),
            reader.GetInt32(7),
            []);

    private static async Task<IReadOnlyList<SeatResponse>> ReadSeatsAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        var seats = new List<SeatResponse>();

        while (await reader.ReadAsync(cancellationToken))
        {
            seats.Add(new SeatResponse(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2)));
        }

        return seats;
    }

    private static void ValidateCreateRequest(CreateShowRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Show name is required.");

        if (request.PricePaise < 0)
            throw new ArgumentException("PricePaise cannot be negative.");

        if (request.Seats is null || request.Seats.Count == 0)
            throw new ArgumentException("At least one seat is required.");
    }

    private static string[] NormalizeSeats(IReadOnlyList<string> seats)
    {
        var normalized = seats.Select(RequestHashUtility.NormalizeSeat).ToArray();

        if (normalized.Any(x => x.Length == 0) ||
            normalized.Distinct(StringComparer.Ordinal).Count() != normalized.Length)
        {
            throw new ArgumentException("Seat numbers must be non-empty and unique.");
        }

        return normalized;
    }
}