# Architecture

## Components
```text
Client
  |
  v
ASP.NET Core API
  |
  +--> Authentication
  +--> Application services
  +--> MySQL data access
          |
          +--> Stored procedures
          +--> InnoDB row locks
```

## Reservation Transaction
1. Authenticate the user.
2. Start one database transaction.
3. Resolve public GUIDs to internal IDs in the same database session/transaction where applicable.
4. Lock required rows.
5. Lock requested seats in deterministic ID order.
6. Validate show ownership, seat availability, MaxSeats, and idempotency.
7. Create reservation and reservation-seat mappings.
8. Update seat status.
9. Commit atomically.

If any requested seat cannot be reserved, the transaction rolls back and no partial reservation remains.

## Concurrency Boundary
A seat row is the inventory locking boundary. SELECT ... FOR UPDATE on requested seats prevents concurrent transactions from both successfully reserving the same seat.

Deterministic lock ordering reduces deadlock risk for overlapping multi-seat requests.

## Identifiers
Public API identifiers are GUID columns named UserGuid, ShowGuid, SeatGuid, and ReservationGuid. Internal BIGINT keys are used for relationships.

## Temporary Tables
When a stored procedure requires multiple input rows, use a connection-scoped MySQL temporary table. Creation, population, procedure execution, and cleanup must use the same connection.

## Data Access
EF Core is not part of the planned data-access layer. Stored procedures are preferred for concurrency-critical operations. Simple parameterized SQL may be used where a stored procedure would add unnecessary complexity.
