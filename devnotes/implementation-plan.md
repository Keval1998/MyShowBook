# Implementation Plan

1. Baseline supplied assignment.
2. Make Docker Compose the primary one-command setup.
3. Complete solution and test project structure.
4. Add MySQL schema, seed data, indexes, and stored procedures.
5. Implement minimal JWT authentication.
6. Implement POST /shows.
7. Implement GET /shows/{id}.
8. Implement POST /shows/{id}/reserve with atomic all-or-nothing booking.
9. Implement owner-only cancellation.
10. Add liveness/readiness, metrics, structured logs, and correlation IDs.
11. Add automated correctness tests.
12. Add one-command 20k-style burst test.
13. Deploy publicly and verify cold start/readiness.
14. Complete WRITEUP.md and final README.
15. Final Agent 2 → Agent 3 → Agent 4 review.

Current milestone: Docker/runtime foundation and liveness.
