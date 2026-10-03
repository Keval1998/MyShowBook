# Decisions

- Use UserGuid, ShowGuid, SeatGuid, and ReservationGuid rather than generic PublicId names.
- Use numeric internal PK/FK values behind public GUIDs.
- Do not add GUIDs to relationship-only tables.
- Keep Show.MaxSeats.
- Use numeric status IDs with EnumStatus where semantics are shared.
- Persist idempotency in the database; service-only checks are insufficient for durable race safety.
- Prefer stored procedures for concurrency-critical operations.
- Do not use EF Core unless a concrete reviewed requirement justifies it.
- Use connection-scoped temporary tables for multi-row stored-procedure inputs.
- Centralize stored-procedure and temporary-table names in constants.
- Agents may use standard MCPs/tools when useful, while remaining within assignment scope.
