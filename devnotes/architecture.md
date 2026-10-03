# Architecture

- ASP.NET Core Web API with MySQL 8/InnoDB.
- No EF Core by default.
- Stored procedures own concurrency-critical transactions.
- Public GUID columns: UserGuid, ShowGuid, SeatGuid, ReservationGuid.
- Internal numeric IDs are used for relationships.
- Resolve GUIDs to internal IDs in the same DB transaction where applicable.
- Lock user/seat rows deterministically.
- Use connection-scoped temporary tables for multi-row SP inputs.
