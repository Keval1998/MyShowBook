# Testing

Required scenarios:
- successful reservation
- multiple-seat reservation
- unavailable seat
- invalid show/seat
- per-show MaxSeats
- failed transaction/rollback
- idempotent retry
- same idempotency key with different request
- authentication/authorization
- cancellation ownership
- concurrent same-seat reservation
- concurrent overlapping multi-seat reservations
- inventory invariant after concurrency

Discussed concurrency target: 20,000 attempts against a hot seat, with one successful reservation and conflicts for the remainder, without unexpected 5xx responses.
