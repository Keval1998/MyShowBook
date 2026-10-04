# MyShowBook

Backend take-home assignment implementation for a seat reservation service.

## Quick Start

Set the required environment values in your shell, then run:

docker compose down -v
docker compose up --build

API/UI: http://localhost:8080

The minimal browser UI is served by the API. It provides local registration, login, admin show creation, show listing/details, customer booking, and admin log viewing. The browser stores the JWT only in sessionStorage and removes it on logout.

## Core API

POST /auth/token
POST /auth/register
POST /shows
GET /shows
GET /shows/{id}
POST /shows/{id}/reserve
POST /reservations/{id}/cancel
GET /health/live
GET /health/ready
GET /metrics
GET /logs (admin only)

## Logging

Serilog writes newline-delimited JSON to the console and to daily rolling files under the logs directory. Docker Compose mounts ./logs into the API container at /app/logs, so logs remain available after container restarts. The admin UI can view the latest 500 log lines through the authenticated /logs endpoint.

Each request has an X-Correlation-ID, and request completion logs include method, path, status and elapsed time. Application events also log authentication and registration outcomes.

## Development Users

admin / admin123! / admin
user1 / user123! / customer
user2 / user456! / customer
user3 / user789! / customer

These credentials are local assignment credentials only. The registration form also permits creating an admin/customer for easy local testing; this is intentionally a minimal assignment/demo feature and should not be exposed as unrestricted production registration.

## Correctness

Reservation decisions are made inside MySQL/InnoDB transactions.

The booking procedure:
1. locks the authenticated user's row;
2. checks persisted idempotency before making a new decision;
3. locks requested seat rows using SELECT ... FOR UPDATE in deterministic seat-number order;
4. validates all seats and the per-user limit;
5. creates the reservation and seat mappings in the same transaction;
6. commits once, or rolls back the complete request.

Multi-seat requests are all-or-nothing.

## Documentation

docs/setup.md — environment and database initialization
docs/architecture.md — architecture
docs/paytm-assignment-requirements.md — assignment requirements
WRITEUP.md — final assignment write-up
devnotes/concepts.md — engineering questions and explanations

## Repository Structure

database/init — table creation
database/seed — enum and development users
database/functions — one stored procedure per file
database/migrations — future timestamped schema changes
src/MyshowBook/Api/Controllers — HTTP endpoints
src/MyshowBook/Api/Helpers — application logic
src/MyshowBook/Api/Utility — shared infrastructure

Run the assignment burst with bash scripts/burst.sh http://localhost:8080 (default 20,000 attempts). Set BURST_COUNT to change the load and ADMIN_USERNAME/ADMIN_PASSWORD/USER_USERNAME/USER_PASSWORD for non-default credentials.