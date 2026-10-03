# Implementation Plan

1. Baseline supplied assignment.
2. Make Docker Compose the primary one-command setup.
3. Add MySQL schema and seed data.
4. Split stored procedures into database/functions, one file per procedure.
5. Keep future ALTER/schema changes in timestamped database/migrations files.
6. Implement minimal JWT authentication.
7. Implement POST /shows.
8. Implement GET /shows/{id}.
9. Implement POST /shows/{id}/reserve with atomic all-or-nothing booking.
10. Implement owner-only cancellation.
11. Add liveness/readiness, metrics, structured logs, and correlation IDs.
12. Manually validate API behavior against the running Docker service.
13. Perform concurrency verification without adding a separate test project unless needed.
14. Add one-command burst test only if final assignment validation needs it.
15. Deploy publicly and verify cold start/readiness.
16. Complete WRITEUP.md.
17. Final Agent 2 -> Agent 3 -> Agent 4 review.

Current milestone: structural refactor and correctness-oriented stored-procedure cleanup based on code review questions.
