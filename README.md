# MyShowBook

Backend take-home assignment implementation for a seat reservation service.

## Quick Start

### Prerequisite

Install Docker Desktop (or Docker Engine + Docker Compose).

### Run locally

Clone the repository, open a terminal in the repository folder, and run:

```bash
docker compose up --build
```

That's it. Docker Compose provides both MySQL and the API. The database is initialized automatically and the API starts after the database is ready.

API/UI: http://localhost:8080

Stop the application with:

```bash
docker compose down
```

### Local configuration

**No environment variables are required for the default local setup.** Docker Compose supplies safe development defaults.

| Variable | Local default | Purpose |
|---|---|---|
| `MYSQL_DATABASE` | `myshowbook` | MySQL database |
| `MYSQL_USER` | `myshowbook` | MySQL application user |
| `MYSQL_PASSWORD` | development default in Compose | MySQL application password |
| `MYSQL_ROOT_PASSWORD` | development default in Compose | MySQL root password |
| `JWT_KEY` | development-only key in Compose | JWT signing key |
| `JWT_ISSUER` | `MyShowBook` | JWT issuer |
| `JWT_AUDIENCE` | `MyShowBook` | JWT audience |
| `JWT_EXPIRY_MINUTES` | `60` | JWT lifetime |

For deployment, do not use these development defaults. Set the corresponding environment variables/secrets in the hosting platform.

### Deployed configuration

The live service is deployed on Render with Aiven MySQL.

Render environment variables are:

- `ConnectionStrings__Default` — Aiven MySQL connection string with SSL enabled
- `Jwt__Key` — secret JWT signing key
- `Jwt__Issuer` — `MyShowBook`
- `Jwt__Audience` — `MyShowBook`
- `Jwt__ExpiryMinutes` — `60`
- `DatabaseBootstrap__Enabled` — `true`
- `DatabaseBootstrap__Path` — `/app/database`
- `PORT` — `8080`
- `ASPNETCORE_HTTP_PORTS` — `8080`

Secrets are intentionally not stored in the repository.

## Demo credentials

These credentials are for local assignment/demo use only:

- Admin: `admin / admin123!`
- Customer: `user1 / user123!`
- Customer: `user2 / user456!`
- Customer: `user3 / user789!`

The UI also allows local registration of admin/customer accounts for testing.

## Core API

```text
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
```

## Quick API/UI test

After `docker compose up --build`, open:

http://localhost:8080

The minimal browser UI supports registration, login, admin show creation, show listing/details, customer booking, cancellation, and admin log viewing.

## Burst / Load Test

The repository includes a one-command hot-seat burst test. It defaults to 20,000 concurrent reservation attempts against the same seat and checks the final reconciliation.

Run locally:

```bash
bash scripts/burst.sh http://localhost:8080
```

Run against the deployed service:

```bash
bash scripts/burst.sh https://myshowbook.onrender.com
```

The script prints confirmed/declined/other HTTP outcomes and fails if the hot-seat or reconciliation expectations are not met.

## Health, Metrics and Logging

- `GET /health/live` — process/liveness check.
- `GET /health/ready` — database-backed readiness check; returns 503 when the database is unavailable.
- `GET /metrics` — Prometheus-style metrics for reservations, decline reasons and available seats.
- `GET /logs` — recent structured logs for authenticated admins.

Serilog writes newline-delimited JSON to the console and daily rolling files under `logs/`. Each request has an `X-Correlation-ID`, and request completion logs include method, path, status and elapsed time.

## Correctness

Reservation decisions are made inside MySQL/InnoDB transactions.

The booking procedure:
1. locks the authenticated user's row;
2. checks persisted idempotency before making a new decision;
3. locks requested seat rows using `SELECT ... FOR UPDATE` in deterministic seat-number order;
4. validates all seats and the per-user limit;
5. creates the reservation and seat mappings in the same transaction;
6. commits once, or rolls back the complete request.

Multi-seat requests are all-or-nothing.

## Documentation

- `docs/setup.md` — setup and database initialization
- `docs/architecture.md` — architecture
- `docs/paytm-assignment-requirements.md` — assignment requirements
- `WRITEUP.md` — final assignment write-up, including AI usage disclosure
- `devnotes/concepts.md` — engineering questions and explanations

## Repository Structure

- `database/init` — table creation
- `database/seed` — enum and development users
- `database/functions` — stored procedures
- `database/migrations` — future timestamped schema changes
- `src/MyshowBook/Api/Controllers` — HTTP endpoints
- `src/MyshowBook/Api/Helpers` — application logic
- `src/MyshowBook/Api/Utility` — shared infrastructure
