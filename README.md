# MyShowBook

Backend take-home assignment implementation for a seat reservation system.

## Overview

MyShowBook exposes a small ASP.NET Core Web API for shows, seats, reservations, and cancellation. The main engineering focus is correctness when many users attempt to reserve the same seat concurrently.

## Technology

- ASP.NET Core Web API
- .NET 10 (current project target)
- MySQL 8 / InnoDB
- Docker / Docker Compose
- Automated .NET tests
- Python asyncio/aiohttp concurrency test

## Key Design Goals

- Atomic multi-seat reservations
- No double booking under concurrency
- Database-backed transaction and row locking
- Durable idempotency
- Per-show booking limit
- JWT-based user identity
- Simple health/readiness and metrics
- Minimal assignment-focused architecture

## Data Access

EF Core is intentionally not used by default. Concurrency-critical operations use MySQL stored procedures so transaction and locking behavior remain explicit.

## Identifiers

The API uses public GUIDs: UserGuid, ShowGuid, SeatGuid, and ReservationGuid. The database may use internal numeric IDs for efficient relationships.

## Quick Start

See [docs/setup.md](docs/setup.md).

## Tests

```bash
dotnet test
```

The high-concurrency reservation scenario is covered by a dedicated load-test script.

## Repository Structure

```text
.github/
  agents/
  skills/
  AGENT-WORKFLOW.md
database/
devnotes/
docs/
load-tests/
src/
tests/
```

## Scope

This project intentionally avoids UI, payment integration, messaging infrastructure, Redis, Kubernetes, Terraform, microservices, and other functionality not required to demonstrate the assignment's core backend concerns.

## Assignment Source

docs/paytm-assignment-requirements.md contains the current working requirement interpretation. If the original assignment email/spec is added to the repository, it becomes authoritative and this document will be reconciled against it.
