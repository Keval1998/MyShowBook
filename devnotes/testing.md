# Testing

## Current smoke checks
- API container starts after MySQL becomes healthy.
- liveness endpoint returns 200.
- readiness checks the database dependency.
- metrics endpoint renders counters and per-show available-seat gauges.
- token issuance works for seeded development users.
- create/get/reserve/cancel flows will be exercised after local Docker startup.

## Required correctness tests
- create show and all seats available
- successful single/multi-seat reservation
- invalid show/seat
- unavailable seat
- all-or-nothing multi-seat request
- default per-user limit of 4
- concurrent per-user limit
- owner cancellation
- non-owner cancellation rejected
- cancelled seat can be booked again
- same idempotency key + same body returns original reservation
- same idempotency key + different seats returns 409
- concurrent duplicate-key requests create one reservation
- hot-seat storm
- overlapping multi-seat storm
- approximately 20,000 concurrent attempts
- zero unexpected 5xx
- reconciliation invariant before/during/after burst
- correlation ID in logs
