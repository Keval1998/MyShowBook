# Architecture

## Components

Client
|
v
ASP.NET Core API
|
+--> JWT authentication / authorization
+--> Controllers
+--> Helpers
+--> Utility/Database
|      |
|      +--> MySqlConnector
|      +--> Stored procedures
|      +--> Temporary tables
|
v
MySQL 8.4 / InnoDB

## Reservation Transaction

1. JWT authentication derives the user identity from the token.
2. The helper opens one MySQL connection.
3. The reservation procedure starts one transaction.
4. The procedure locks the authenticated user's row. This serializes that user's concurrent reservation/cancellation decisions and protects the per-user limit/idempotency check.
5. Requested seat rows are locked with SELECT ... FOR UPDATE in seat-number order.
6. The procedure checks requested-seat existence, seat status, idempotency, and the per-user limit.
7. Reservation and seat mappings are written atomically.
8. The procedure commits.

If any requested seat fails validation, the whole transaction rolls back.

## Concurrency

A seat row is the inventory locking boundary. MySQL releases FOR UPDATE locks only when the transaction commits or rolls back. citeturn6search0

The same user row is locked before the seat rows. This serializes that user's competing reservations and cancellation requests.

Multi-seat seat locks are requested in deterministic seat-number order. Consistent lock order is used to reduce deadlock risk. MySQL recommends acquiring locks in a consistent order when modifying multiple sets of rows. citeturn6search2

## Data Access

EF Core is intentionally not used.

Utility/Database contains the generic stored-procedure execution path. Helpers provide only business-specific validation and result mapping.

Concurrency-critical operations remain in stored procedures.

## Identifiers

Public GUIDs:
- UserGuid
- ShowGuid
- SeatGuid
- ReservationGuid

Internal BIGINT IDs support relationships and database locking.

## Temporary Tables

When a procedure needs multiple input rows, the API creates a connection-scoped temporary table, populates it, and calls the procedure on the same connection.

Current input tables:
- tmp_show_seats
- tmp_reservation_seats

## Database Organization

database/init
- initial table definitions

database/seed
- enum values
- development users

database/functions
- one stored procedure per file

database/migrations
- future timestamped schema changes such as ALTER TABLE

database/00-run-init.sh
- executes init -> seed -> functions -> migrations during fresh MySQL initialization
