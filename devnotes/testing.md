# Testing

## Functional
- create show
- all created seats start available
- successful single/multi-seat reservation
- invalid show/seat
- unavailable seat
- all-or-nothing multi-seat request
- default per-user limit of 4
- concurrent per-user limit
- owner cancellation
- non-owner cancellation rejected
- cancelled seat can be booked again

## Idempotency
- same user + same key + same body returns original reservation
- same user + same key + different seats returns 409
- concurrent duplicate-key requests create one reservation

## Concurrency
- hot-seat storm
- overlapping multi-seat storm
- approximately 20,000 concurrent attempts
- zero unexpected 5xx
- exactly one winner per hot seat
- reconciliation invariant before/during/after burst

## Observability
- liveness
- readiness fails when DB is unavailable
- confirmed counter
- decline counters by reason
- available-seat gauge
- correlation/request ID in logs
