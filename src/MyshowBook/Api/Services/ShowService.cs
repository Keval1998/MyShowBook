using MyShowBook.Api.Constants;
using MyShowBook.Api.Database;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;

namespace MyShowBook.Api.Services;

public sealed class ShowService(
    IDbConnectionFactory connectionFactory,
    TempTableHelper tempTableHelper)
{
    public async Task<ShowResponse> CreateAsync(
        CreateShowRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCreateRequest(request);
        var seats = NormalizeSeats(request.Seats);

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await tempTableHelper.CreateSeatTableAsync(
            connection, TempTableNames.ShowSeats, seats, cancellationToken);

        await using var command = StoredProcedureHelper.Create(
            connection,
            StoredProcedureNames.CreateShow,
            StoredProcedureHelper.StringParameter("p_show_guid", Guid.NewGuid().ToString("D")),
            StoredProcedureHelper.StringParameter("p_name", request.Name.Trim()),
            StoredProcedureHelper.LongParameter("p_price_paise", request.PricePaise),
            StoredProcedureHelper.IntParameter("p_per_user_limit", 4));

        return await ReadShowAsync(command, cancellationToken);
    }

    public async Task<ShowResponse?> GetAsync(
        Guid showGuid,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await using var command = StoredProcedureHelper.Create(
            connection,
            StoredProcedureNames.GetShow,
            StoredProcedureHelper.StringParameter("p_show_guid", showGuid.ToString("D")));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return await ReadCurrentShowAsync(reader, cancellationToken);
    }

    private static async Task<ShowResponse> ReadShowAsync(
        MySqlConnector.MySqlCommand command,
        CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Show procedure returned no show.");

        return await ReadCurrentShowAsync(reader, cancellationToken);
    }

    private static async Task<ShowResponse> ReadCurrentShowAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        var response = new ShowResponse(
            reader.GetGuidFromString(0),
            reader.GetString(1),
            reader.GetInt64(2),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.GetInt32(5),
            reader.GetInt32(6),
            reader.GetInt32(7),
            []);

        if (!await reader.NextResultAsync(cancellationToken))
            return response;

        var seats = new List<SeatResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            seats.Add(new SeatResponse(
                reader.GetGuidFromString(0),
                reader.GetString(1),
                reader.GetString(2)));
        }

        return response with { Seats = seats };
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
        var normalized = seats.Select(RequestHashHelper.NormalizeSeat).ToArray();

        if (normalized.Any(x => x.Length == 0) ||
            normalized.Distinct(StringComparer.Ordinal).Count() != normalized.Length)
        {
            throw new ArgumentException("Seat numbers must be non-empty and unique.");
        }

        return normalized;
    }
}
