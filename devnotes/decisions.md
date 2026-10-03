# Decisions

- Use UserGuid, ShowGuid, SeatGuid, ReservationGuid rather than generic PublicId names.
- Use internal numeric PK/FK values behind public GUIDs.
- Do not add GUIDs to relationship-only tables.
- Use a per-user booking limit of 4 by default; this is not MaxSeats.
- Use all-or-nothing multi-seat reservations.
- Use explicit owner-only cancellation rather than automatic hold expiry.
- Use MySQL/InnoDB transactions and deterministic row locking for reservation correctness.
- Persist idempotency in the database.
- No EF Core by default.
- Prefer stored procedures for concurrency-critical operations.
- Money is integer paise.
- Docker Compose is the primary clean-checkout setup path.
- Public deployment is required.
