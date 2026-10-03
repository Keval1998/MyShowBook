# Paytm Assignment Requirements

> Working requirement record reconstructed from the assignment discussion. If the original Paytm email/spec is added to the repository, it supersedes this document.

## Required Scope
- Backend Web API.
- Required show/event creation and retrieval.
- Seat availability.
- Seat reservation.
- Required reservation cancellation/details.
- JWT authentication.
- Per-show MaxSeats booking limit.
- Atomic multi-seat reservation.
- No double booking under concurrent requests.
- MySQL 8 / InnoDB transaction and row locking.
- Idempotent reservation retry behavior.
- Liveness/readiness and required metrics/observability.
- Automated tests.
- High-concurrency reservation testing; 20,000 concurrent attempts is the discussed target.
- Docker/Docker Compose.
- Clean README and setup/write-up documentation.

## Correctness
The database is the inventory source of truth.

For a multi-seat reservation:
- all seats must belong to the target show;
- all requested seats must be available;
- the booking limit must be respected;
- either all requested seats are reserved or none;
- concurrent attempts cannot create multiple successful reservations for one seat.

## Idempotency
Persist the idempotency boundary in the database.

For the same user and idempotency key:
- the same request returns the same logical reservation result;
- a different request is rejected as a conflict.

A database uniqueness constraint is the durable race-safety boundary.

## Data Access
No EF Core by default. Prefer stored procedures for concurrency-critical operations.

## Identifiers
Public GUID columns:
- UserGuid
- ShowGuid
- SeatGuid
- ReservationGuid

Internal numeric IDs may be used for joins and locking.

## Status
Use numeric status IDs backed by EnumStatus where statuses have compatible semantics.

## Multi-row Stored Procedure Inputs
Use connection-scoped temporary tables when a stored procedure needs multiple input rows. Centralize temporary-table handling and stored-procedure names in small utilities/constants.

## Explicitly Out of Scope
Unless the original assignment says otherwise:
- UI
- Payment gateway
- Notifications
- Kafka/RabbitMQ
- Redis
- Kubernetes/Terraform
- Microservices
- Elaborate authentication/authorization platform
- Refresh-token ecosystem
- Enterprise Clean Architecture
- Unrequested infrastructure
