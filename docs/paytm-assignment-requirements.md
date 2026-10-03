# Paytm Assignment Requirements

This document is based on the supplied Paytm assignment email and is the implementation source of truth.

## Functional API

### POST /shows
Admin endpoint. Request contains `name`, assigned seat names, and `price_paise`. Created seats start as available.

### POST /shows/{id}/reserve
Authenticated endpoint. Request contains requested seats and an idempotency key. Identity comes from the auth token, not the body.

Required behavior:
- no seat can be confirmed for two users;
- hot-seat races produce one 201 and clean 409 declines;
- expected booking conflicts never become 5xx;
- default per-user limit is 4 seats per show;
- the limit holds under concurrency;
- same key and same request returns the original reservation;
- same key with different requested seats returns 409;
- multi-seat behavior must be explicitly defined and concurrency-safe.

This implementation chooses **all-or-nothing** multi-seat behavior: if any requested seat is invalid or unavailable, no seat from that request is reserved.

### POST /reservations/{id}/cancel
This implementation chooses explicit owner-only cancellation rather than time-boxed expiry. A cancelled seat becomes available again and cancellation cannot overwrite another user's confirmed reservation.

### GET /shows/{id}
Returns per-seat state and counts. The reconciliation invariant is always:

`available + held + confirmed == total_seats`

With the explicit-cancellation model and no temporary hold state, held may remain zero.

## Correctness Bar

The assignment expects approximately 20,000 concurrent reservation attempts. Required properties:

1. No seat is confirmed to two users.
2. Zero unexpected 5xx during the burst.
3. Reconciliation holds during and after the burst.
4. Idempotent retries create no extra reservation.
5. Per-user limit holds under concurrency.
6. Identity is token-derived and cancellation is owner-only.

The atomic decision must live in a race-safe mechanism such as row locks, conditional updates, or uniqueness constraints; read-then-write is insufficient.

## Deploy & Observe

- Public deployment with a live URL.
- Dockerfile and Compose.
- Liveness endpoint.
- Readiness endpoint that checks DB reachability and fails closed if DB is unavailable.
- Prometheus-style metrics: confirmed counter, decline counters including seat-taken/per-user-limit/idempotent-replay, and seats-available gauge.
- Structured logs with correlation/request ID.
- One-command burst script against a supplied base URL, including hot-seat contention, outcome distribution, and final reconciliation.

## Deliverables

- Public Git repo with full commit history.
- Public live URL.
- One-command burst script documented in README.
- Metrics and logs access.
- `WRITEUP.md` covering atomic decision/race safety, multi-seat deadlock avoidance, idempotency, release/expiry model, consistency vs availability during partition, observability/2am alerts, honest AI usage, and next steps.

## Ground Rules

- Money is integer minor units (paise), never floats.
- AI tools are allowed and expected and usage must be disclosed honestly.
- Clean checkout must build and run.
- The running service is the primary grading target.

## Scope Principle

Use the simplest design that satisfies correctness and deploy/observe requirements. Avoid unrelated infrastructure and features.
