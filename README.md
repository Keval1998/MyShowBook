# MyShowBook

Backend take-home assignment implementation for a seat reservation service.

## Quick Start

Run:

docker compose up --build

API: http://localhost:8080

After changing database initialization SQL:

docker compose down -v
docker compose up --build

## Core API

POST /auth/token
POST /shows
POST /shows/{id}/reserve
POST /reservations/{id}/cancel
GET /shows/{id}
GET /health/live
GET /health/ready
GET /metrics

The token endpoint is a minimal development authentication mechanism so the assignment API is directly runnable. Production deployment must override Jwt:Key with a deployment secret.

Identity for reservation/cancellation comes from the JWT, not the request body. Money uses integer paise.

## Correctness

Reservation decisions are made inside MySQL/InnoDB transactions.

The booking procedure:
1. locks the authenticated user's row;
2. checks persisted idempotency before making a new decision;
3. locks requested seat rows in deterministic seat-number order using the show/seat unique index;
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

docs/laptop-setup-commands.md — copy/paste setup commands
docs/setup.md — environment and database initialization
docs/architecture.md — architecture
docs/paytm-assignment-requirements.md — assignment requirements
WRITEUP.md — final assignment write-up

## Repository Structure

database/init — table creation
database/seed — enum and development users
database/migrations — stored procedures
src/MyshowBook/Api — API source
devnotes — persistent engineering context
docs — project documentation

Automated correctness tests and the 20k-style burst test are the next milestones.
