# MyShowBook

Backend take-home assignment implementation for a seat reservation service.

## Quick Start

A clean checkout should run with Docker:

```bash
docker compose up --build
```

Stop with `docker compose down`. Reset local data with `docker compose down -v`.

## Assignment

The service implements Paytm's Seat Reservation at Scale exercise: atomic assigned-seat reservation under concurrency, per-user limits, durable idempotency, owner-only cancellation, reconciliation, health/readiness, Prometheus metrics, structured logs, a one-command burst test, and public deployment.

## Core API

```text
POST /shows
POST /shows/{id}/reserve
POST /reservations/{id}/cancel
GET  /shows/{id}
GET  /health/live
GET  /health/ready
GET  /metrics
```

Identity comes from the authentication token. Money is integer paise, never floating point.

## Correctness

Reservation decisions are atomic in MySQL/InnoDB. Requested seats are locked in deterministic order. Multi-seat requests use all-or-nothing semantics: if any requested seat is unavailable or invalid, the whole request is declined.

Public API identifiers use `UserGuid`, `ShowGuid`, `SeatGuid`, and `ReservationGuid`; internal numeric IDs may be used for database relationships.

## Documentation

- [Setup](docs/setup.md)
- [Architecture](docs/architecture.md)
- [Assignment requirements](docs/paytm-assignment-requirements.md)
- [Write-up](WRITEUP.md)

## Tests

```bash
dotnet test
```

The high-concurrency burst test lives under `load-tests/` and reports confirmed, decline reasons, 5xx responses, and final reconciliation.

## AI Usage

AI tools are used as engineering assistants for requirement analysis, design discussion, implementation support, code review, testing strategy, and documentation. Human review remains responsible for final decisions and submitted code. Details are documented in `WRITEUP.md`.

## Repository Structure

```text
.github/       agent roles and skills
database/      MySQL schema, seed data, stored procedures
devnotes/      persistent engineering context
docs/          setup, architecture, assignment requirements
load-tests/    concurrency burst test
src/           API source
tests/         automated tests
Dockerfile
docker-compose.yml
WRITEUP.md
```
