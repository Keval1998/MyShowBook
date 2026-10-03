# Implementation Plan

1. Baseline supplied assignment.
2. Make Docker Compose the primary one-command setup.
3. Add MySQL schema, seed data, indexes, and stored procedures.
4. Implement minimal JWT authentication.
5. Implement POST /shows.
6. Implement GET /shows/{id}.
7. Implement POST /shows/{id}/reserve with atomic all-or-nothing booking.
8. Implement owner-only cancellation.
9. Add liveness/readiness, metrics, structured logs, and correlation IDs.
10. Add automated correctness tests.
11. Add one-command 20k-style burst test.
12. Deploy publicly and verify cold start/readiness.
13. Complete WRITEUP.md and final README.
14. Final Agent 2 -> Agent 3 -> Agent 4 review.

Current milestone: database foundation, minimal authentication, core API, and observability scaffolding.
