# Architecture

- ASP.NET Core Web API with MySQL 8/InnoDB.
- Docker Compose is the primary clean-checkout setup path.
- No EF Core by default.
- Stored procedures own concurrency-critical transactions.
- Public GUIDs: UserGuid, ShowGuid, SeatGuid, ReservationGuid.
- Internal numeric IDs support relationships and locking.
- GUID resolution occurs within the same DB transaction/session where required.
- Requested seats are locked deterministically.
- Multi-seat requests are all-or-nothing.
- Idempotency is persisted in the database.
- Explicit owner-only cancellation is used instead of automatic expiry.
