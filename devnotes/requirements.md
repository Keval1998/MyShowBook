# Requirements

- Keep API scope minimal and assignment-specific.
- Reservation correctness and concurrency are the primary engineering concern.
- JWT identity comes from authentication.
- MaxSeats belongs to Show.
- Multi-seat reservation is atomic.
- Idempotency is durable and database-backed.
- MySQL/InnoDB is the source of truth.
