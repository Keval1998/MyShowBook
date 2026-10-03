# Decisions

- Use UserGuid, ShowGuid, SeatGuid, ReservationGuid for public identifiers; internal BIGINT IDs remain for relationships and locking.
- Per-user booking limit is 4 by default.
- Multi-seat reservation semantics are all-or-nothing.
- Release model is explicit owner-only cancellation.
- MySQL/InnoDB is the source of truth.
- Reservation correctness uses one transaction, user-row serialization for per-user/idempotency races, and deterministic seat-number row locking.
- Idempotency is persisted on Reservations with a request hash and unique (UserId, IdempotencyKey).
- Temporary tables are used for multi-seat stored-procedure inputs and created/populated on the same MySQL connection used to call the procedure.
- Stored procedure names and temporary table names are centralized in constants.
- No EF Core.
- Money is integer paise.
- Docker Compose is the primary clean-checkout setup path.
- Minimal JWT token issuance exists only to make the authenticated assignment API directly runnable.
- No automatic hold expiry; held status remains defined but unused because the selected release model is explicit cancellation.
