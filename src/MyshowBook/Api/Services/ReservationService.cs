using MyShowBook.Api.Constants;
using MyShowBook.Api.Database;
using MyShowBook.Api.Enums;
using MyShowBook.Api.Helpers;
using MyShowBook.Api.Models;

namespace MyShowBook.Api.Services;

public sealed class ReservationService(
    IDbConnectionFactory connectionFactory,
    TempTableHelper tempTableHelper,
    MetricsService metrics)
{
    public async Task<ReservationResult> ReserveAsync(
        Guid userGuid,
        Guid showGuid,
        ReserveRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var seats = NormalizeSeats(request.Seats);
        var requestHash = RequestHashHelper.Create(showGuid, seats);

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await tempTableHelper.CreateSeatTableAsync(
            connection, TempTableNames.ReservationSeats, seats, cancellationToken);

        await using var command = StoredProcedureHelper.Create(
            connection,
            StoredProcedureNames.CreateReservation,
            StoredProcedureHelper.StringParameter("p_user_guid", userGuid.ToString("D")),
            StoredProcedureHelper.StringParameter("p_show_guid", showGuid.ToString("D")),
            StoredProcedureHelper.StringParameter("p_idempotency_key", request.IdempotencyKey.Trim()),
            StoredProcedureHelper.StringParameter("p_request_hash", requestHash),
            StoredProcedureHelper.StringParameter("p_reservation_guid", Guid.NewGuid().ToString("D")));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Reservation procedure returned no result.");

        var outcome = reader.GetString(0);

        if (outcome is "CONFIRMED" or "IDEMPOTENT_REPLAY")
        {
            var reservation = await ReadReservationAsync(reader, cancellationToken);

            if (outcome == "CONFIRMED")
                metrics.RecordConfirmed();
            else
                metrics.RecordDeclined(DeclineReason.IdempotentReplay);

            return new ReservationResult(outcome, reservation);
        }

        if (outcome == "SEAT_TAKEN")
            metrics.RecordDeclined(DeclineReason.SeatTaken);
        else if (outcome == "PER_USER_LIMIT")
            metrics.RecordDeclined(DeclineReason.PerUserLimit);

        return new ReservationResult(outcome, null);
    }

    public async Task<ReservationResult> CancelAsync(
        Guid userGuid,
        Guid reservationGuid,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await using var command = StoredProcedureHelper.Create(
            connection,
            StoredProcedureNames.CancelReservation,
            StoredProcedureHelper.StringParameter("p_user_guid", userGuid.ToString("D")),
            StoredProcedureHelper.StringParameter("p_reservation_guid", reservationGuid.ToString("D")));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Cancellation procedure returned no result.");

        var outcome = reader.GetString(0);
        if (outcome != "CANCELLED")
            return new ReservationResult(outcome, null);

        return new ReservationResult(
            outcome,
            await ReadReservationAsync(reader, cancellationToken));
    }

    private static async Task<ReservationResponse> ReadReservationAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        var response = new ReservationResponse(
            reader.GetGuidFromString(1),
            reader.GetGuidFromString(2),
            reader.GetGuidFromString(3),
            [],
            reader.GetInt64(4),
            reader.GetString(5));

        var seats = new List<string>();
        do
        {
            seats.Add(reader.GetString(6));
        }
        while (await reader.ReadAsync(cancellationToken));

        return response with { Seats = seats };
    }

    private static void ValidateRequest(ReserveRequest request)
    {
        if (request.Seats is null || request.Seats.Count == 0)
            throw new ArgumentException("At least one seat is required.");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ArgumentException("IdempotencyKey is required.");

        if (request.IdempotencyKey.Length > 128)
            throw new ArgumentException("IdempotencyKey must be 128 characters or fewer.");
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

public sealed record ReservationResult(
    string Outcome,
    ReservationResponse? Reservation);
