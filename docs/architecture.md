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

1. Authenticate and derive user identity from the token.
2. Start one DB transaction.
3. Resolve public GUIDs to internal IDs in the same DB session/transaction where applicable.
4. Lock user/show state needed for the booking limit.
5. Lock requested seats in deterministic ID order.
6. Check idempotency, seat ownership/state, and per-user limit.
7. Write reservation and seat mappings atomically.
8. Commit.

If any requested seat fails validation, the whole transaction rolls back.

## Concurrency

A seat row is the inventory locking boundary. `SELECT ... FOR UPDATE` prevents concurrent transactions from both confirming the same seat. Deterministic lock ordering reduces deadlock risk for overlapping multi-seat requests.

## Data Access

EF Core is not planned. Stored procedures own concurrency-critical operations. Simple parameterized SQL may be used for straightforward reads.

## Identifiers

Public GUIDs: `UserGuid`, `ShowGuid`, `SeatGuid`, `ReservationGuid`. Internal BIGINT IDs support relationships and locking.

## Temporary Tables

When a procedure needs multiple input rows, use a connection-scoped MySQL temporary table. Creation, population, procedure execution, and cleanup must use the same connection.
