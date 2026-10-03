using MyShowBook.Api.Constants;
using MyShowBook.Api.Enums;
using MyShowBook.Api.Models;
using MyShowBook.Api.Utility;
using MyShowBook.Api.Utility.Database;

namespace MyShowBook.Api.Helpers;

public sealed class ReservationHelper(
    DatabaseUtility database,
    TemporaryTableUtility temporaryTables,
    MetricsHelper metrics)
{
    public Task<ReservationResult> ReserveAsync(
        Guid userGuid,
        Guid showGuid,
        ReserveRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var seats = NormalizeSeats(request.Seats);
        var requestHash = RequestHashUtility.Create(showGuid, seats);

        return database.ExecuteProcedureAsync(
            StoredProcedureNames.CreateReservation,
            [
                StoredProcedureUtility.String("p_user_guid", userGuid.ToString("D")),
                StoredProcedureUtility.String("p_show_guid", showGuid.ToString("D")),
                StoredProcedureUtility.String("p_idempotency_key", request.IdempotencyKey.Trim()),
                StoredProcedureUtility.String("p_request_hash", requestHash),
                StoredProcedureUtility.String("p_reservation_guid", Guid.NewGuid().ToString("D"))
            ],
            async (reader, ct) =>
            {
                var result = await ReadResultAsync(reader, ct);

                if (result.Outcome == ReservationOutcome.Confirmed)
                    metrics.RecordConfirmed();
                else if (result.Outcome == ReservationOutcome.SeatTaken)
                    metrics.RecordDeclined(DeclineReason.SeatTaken);
                else if (result.Outcome == ReservationOutcome.PerUserLimit)
                    metrics.RecordDeclined(DeclineReason.PerUserLimit);
                else if (result.Outcome == ReservationOutcome.IdempotentReplay)
                    metrics.RecordDeclined(DeclineReason.IdempotentReplay);

                return result;
            },
            (connection, ct) => temporaryTables.CreateSeatInputAsync(
                connection, TempTableNames.ReservationSeats, seats, ct),
            cancellationToken);
    }

    public Task<ReservationResult> CancelAsync(
        Guid userGuid,
        Guid reservationGuid,
        CancellationToken cancellationToken) =>
        database.ExecuteProcedureAsync(
            StoredProcedureNames.CancelReservation,
            [
                StoredProcedureUtility.String("p_user_guid", userGuid.ToString("D")),
                StoredProcedureUtility.String("p_reservation_guid", reservationGuid.ToString("D"))
            ],
            ReadCancellationResultAsync,
            cancellationToken: cancellationToken);

    private static async Task<ReservationResult> ReadResultAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        await SkipLockResultAsync(reader, cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Reservation procedure returned no outcome.");

        var outcome = (ReservationOutcome)reader.GetInt32(0);

        if (outcome is not (ReservationOutcome.Confirmed or ReservationOutcome.IdempotentReplay))
            return new ReservationResult(outcome, null);

        var reservation = await ReadReservationAsync(reader, cancellationToken);
        return new ReservationResult(outcome, reservation);
    }

    private static async Task<ReservationResult> ReadCancellationResultAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        await SkipLockResultAsync(reader, cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Cancellation procedure returned no outcome.");

        var outcome = (CancellationOutcome)reader.GetInt32(0);

        if (outcome != CancellationOutcome.Cancelled)
            return new ReservationResult(
                ReservationOutcome.InvalidSeat,
                null);

        var reservation = await ReadReservationAsync(reader, cancellationToken);
        return new ReservationResult(
            ReservationOutcome.Confirmed,
            reservation);
    }

    private static async Task SkipLockResultAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        if (reader.FieldCount == 0 || reader.GetName(0) != "Outcome")
        {
            while (await reader.ReadAsync(cancellationToken))
            {
            }

            if (!await reader.NextResultAsync(cancellationToken))
                throw new InvalidOperationException("Procedure returned no outcome result set.");
        }
    }

    private static async Task<ReservationResponse> ReadReservationAsync(
        MySqlConnector.MySqlDataReader reader,
        CancellationToken cancellationToken)
    {
        var response = new ReservationResponse(
            reader.GetGuid(1),
            reader.GetGuid(2),
            reader.GetGuid(3),
            [],
            reader.GetInt64(4),
            reader.GetString(5));

        if (!await reader.NextResultAsync(cancellationToken))
            return response;

        var seats = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
            seats.Add(reader.GetString(0));

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
        var normalized = seats.Select(RequestHashUtility.NormalizeSeat).ToArray();

        if (normalized.Any(x => x.Length == 0) ||
            normalized.Distinct(StringComparer.Ordinal).Count() != normalized.Length)
        {
            throw new ArgumentException("Seat numbers must be non-empty and unique.");
        }

        return normalized;
    }
}
