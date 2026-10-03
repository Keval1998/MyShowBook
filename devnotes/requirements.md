# Requirements

Source: supplied Paytm assignment email.

## Required
- JSON HTTP API: create show, reserve, cancel/release, show state.
- Token-derived identity and owner-only cancellation.
- Default per-user limit of 4 seats per show.
- Atomic multi-seat behavior; chosen model is all-or-nothing.
- Durable idempotency.
- MySQL/InnoDB concurrency correctness.
- Liveness and DB-backed readiness.
- Prometheus metrics.
- Structured logs with correlation/request ID.
- Dockerfile/Compose and public deployment.
- One-command concurrency burst.
- README and WRITEUP.md.

## Correctness invariant
`available + held + confirmed == total_seats`.
Expected booking conflicts are 4xx outcomes, not 5xx errors.
