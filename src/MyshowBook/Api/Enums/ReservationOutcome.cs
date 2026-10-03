namespace MyShowBook.Api.Enums;

public enum ReservationOutcome
{
    Confirmed = 1,
    IdempotentReplay = 2,
    SeatTaken = 3,
    PerUserLimit = 4,
    IdempotencyConflict = 5,
    InvalidSeat = 6,
    ShowNotFound = 7,
    UserNotFound = 8
}
