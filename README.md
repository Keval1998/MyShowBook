# MyShowBook

Backend take-home assignment implementation for a seat reservation service.

## Quick Start

Set the required environment values in your shell, then run:

docker compose down -v
docker compose up --build

API: http://localhost:8080

## Core API

POST /auth/token
POST /shows
POST /shows/{id}/reserve
POST /reservations/{id}/cancel
GET /shows/{id}
GET /health/live
GET /health/ready
GET /metrics

The token endpoint is a minimal development authentication mechanism so the assignment API is directly runnable. Production deployment must provide a secure JWT signing key through environment configuration.

Identity for reservation/cancellation comes from the JWT, not the request body. Money uses integer paise.

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

## Development Users

admin / admin123! / admin
user1 / user123! / user
user2 / user456! / user
user3 / user789! / user

These credentials are local assignment credentials only.

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
src/MyshowBook/Api/Models — request/response/result models
src/MyshowBook/Api/Utility — shared infrastructure

Run the assignment burst with `bash scripts/burst.sh http://localhost:8080` (default 20,000 attempts). Set `BURST_COUNT` to change the load and `ADMIN_USERNAME`/`ADMIN_PASSWORD`/`USER_USERNAME`/`USER_PASSWORD` for non-default credentials.
