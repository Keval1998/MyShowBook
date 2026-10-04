# Seat Reservation at Scale — Write-up

## 1. Architecture and atomic reservation mechanism

The service uses ASP.NET Core with MySQL 8.4/InnoDB as the source of truth. Public API identifiers are GUIDs while internal database relationships use numeric IDs.

Reservation decisions are executed by the `sp_create_reservation` stored procedure inside a single database transaction.

The transaction:
1. Resolves and locks the authenticated user row first.
2. Checks the persisted idempotency record for the user and key.
3. Validates the request and requested seat set.
4. Locks the requested seat rows with `SELECT ... FOR UPDATE`, always in deterministic seat-number order.
5. Validates that every requested seat is available and that the per-user confirmed-seat limit is not exceeded.
6. Creates the reservation and reservation-seat mappings.
7. Records the idempotency result and commits once.

If any validation fails, the transaction is rolled back. This gives all-or-nothing behavior for multi-seat reservations and prevents two concurrent transactions from confirming the same seat.

The database transaction is deliberately the correctness boundary: application-level checks are not treated as sufficient for preventing races.

## 2. Hot-seat concurrency and deadlock avoidance

Concurrent requests for the same seat serialize on the InnoDB row lock. Exactly one transaction can observe and confirm the seat as available; subsequent transactions see the updated state and decline with HTTP 409.

For multi-seat requests, seat rows are always locked in deterministic seat-number order. The user row is acquired before seat locks. Cancellation follows the compatible user → reservation → seat order. This consistent ordering reduces deadlock risk.

Transient MySQL lock/deadlock errors (1205/1213) are retried by the database utility for a bounded number of attempts. This is recovery for transient contention, not a substitute for the transaction's correctness guarantees.

The repository contains a one-command 20,000-attempt hot-seat burst. The final successful GitHub Actions run confirmed:

- 20,000 concurrent attempts
- 1 HTTP 201 winner
- 19,999 HTTP 409 declines
- 0 other/5xx responses
- final show reconciliation passed

## 3. Idempotency

Idempotency is persisted in the database using the authenticated user and idempotency key.

A retry with the same key and equivalent request returns the original reservation result rather than creating another reservation.

Reusing the same key with a different request body is rejected as an idempotency conflict. The request hash is persisted so this decision survives application restarts.

The idempotency record participates in the same database transaction as the reservation decision, so the reservation and its idempotency result cannot become independently committed.

## 4. Multi-seat behavior

Multi-seat reservation is explicitly all-or-nothing.

If any requested seat is unavailable, the request is declined and no subset of the seats is sold.

This behavior is intentionally simple and deterministic for the assignment.

## 5. Cancellation

Cancellation is explicit rather than time-based.

Only the reservation owner can cancel a confirmed reservation. Cancellation is performed transactionally and uses the compatible lock ordering described above.

There is no background hold-expiry workflow because the assignment scope does not require temporary reservation expiry.

## 6. Consistency versus availability

MySQL/InnoDB is the authoritative source for seat state. The system chooses correctness and consistency over accepting potentially unsafe writes during a database partition.

If the database cannot be reached, readiness fails and the application does not pretend that reservations are safe to accept.

The readiness endpoint performs an actual database connectivity check and returns HTTP 503 when the database is unavailable.

## 7. Observability

The API exposes:

- `GET /health/live` — process/liveness check.
- `GET /health/ready` — database-backed readiness check.
- `GET /metrics` — Prometheus metrics.
- `GET /logs` — admin-only recent structured logs.

Prometheus metrics cover confirmed reservations, decline categories, and available-seat measurements.

Serilog writes structured JSON logs to console and rolling files. Requests receive correlation/request identifiers, and completion logs include HTTP method, path, status code, and elapsed time.

For an operational incident, the most important alerts are database readiness failures, elevated HTTP 5xx rates, reservation-decline anomalies, and a mismatch between expected seat totals and confirmed/available counts.

## 8. Deployment

The application is packaged with the repository Dockerfile.

Local development uses Docker Compose with MySQL 8.4. Compose waits for the database health check before starting the API.

The deployed service uses Render's Docker runtime with Aiven MySQL as the external database. Database initialization is automated by the API when `DatabaseBootstrap:Enabled=true`. The repository's schema, seed data, and stored procedures are included in the application image.

The bootstrapper:
- connects to the configured external MySQL database;
- creates the schema when needed;
- skips already-existing schema/seed work;
- safely replaces the repository's stored procedures;
- records a completion marker;
- fails application startup if required bootstrap work cannot complete.

The final deployed service uses `/health/ready` as its Render health-check path.

## 9. Verification performed

The repository's Compose Runtime Smoke workflow passed on the final main revision, including build, startup, readiness, liveness, metrics, and registration smoke.

The Assignment Load Test workflow also passed on the final main revision with the 20,000-request hot-seat burst and reconciliation.

The Render deployment was independently verified from the live deployment logs. The final deployment:
- built the Docker image successfully;
- connected to the existing Aiven schema;
- completed database bootstrap successfully;
- started the API on port 8080;
- returned HTTP 200 from `/health/ready`;
- was marked live by Render;
- published the primary URL `https://myshowbook.onrender.com`.

## 10. AI usage disclosure

AI assistance was used for code review, requirement cross-checking, debugging, test/verification workflow design, deployment troubleshooting, and documentation refinement.

The implementation decisions were reviewed against the assignment requirements and repository behavior. In particular, the final concurrency mechanism, transaction boundaries, lock ordering, idempotency model, cancellation model, and consistency behavior were selected to satisfy the assignment rather than blindly accepting generated suggestions.

## 11. Next improvements

Outside assignment scope, production hardening could include:
- a proper migration framework/versioned migration files;
- managed secret rotation and secret storage;
- persistent ASP.NET Data Protection keys;
- rate limiting at the edge;
- distributed tracing;
- stronger automated integration tests for idempotency and cancellation;
- database backup/restore automation;
- more granular operational dashboards;
- a dedicated hold/expiry workflow if temporary seat holds become a product requirement.

For the take-home assignment, the current implementation deliberately favors a small, explainable correctness boundary over unnecessary infrastructure.
