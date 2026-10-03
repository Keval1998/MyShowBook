using System.Collections.Concurrent;
using System.Text;
using MyShowBook.Api.Constants;
using MyShowBook.Api.Database;
using MyShowBook.Api.Enums;

namespace MyShowBook.Api.Services;

public sealed class MetricsService(IDbConnectionFactory connectionFactory)
{
    private long _confirmed;
    private readonly ConcurrentDictionary<DeclineReason, long> _declined = new();

    public void RecordConfirmed() => Interlocked.Increment(ref _confirmed);

    public void RecordDeclined(DeclineReason reason) =>
        _declined.AddOrUpdate(reason, 1, (_, current) => current + 1);

    public async Task<string> RenderAsync(CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        builder.AppendLine("# TYPE reservations_confirmed_total counter");
        builder.AppendLine($"reservations_confirmed_total {Interlocked.Read(ref _confirmed)}");
        builder.AppendLine("# TYPE reservations_declined_total counter");

        foreach (var reason in Enum.GetValues<DeclineReason>())
        {
            var value = _declined.TryGetValue(reason, out var count) ? count : 0;
            var label = reason switch
            {
                DeclineReason.SeatTaken => "seat-taken",
                DeclineReason.PerUserLimit => "per-user-limit",
                DeclineReason.IdempotentReplay => "idempotent-replay",
                _ => reason.ToString()
            };
            builder.AppendLine($"reservations_declined_total{{reason="{label}"}} {value}");
        }

        builder.AppendLine("# TYPE seats_available gauge");

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await using var command = StoredProcedureHelper.Create(
            connection, StoredProcedureNames.GetAvailableSeatsMetrics);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var showGuid = reader.GetGuidFromString(0);
            var available = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            builder.AppendLine($"seats_available{{show_id="{showGuid:D}"}} {available}");
        }

        return builder.ToString();
    }
}
