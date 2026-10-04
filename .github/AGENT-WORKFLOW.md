# Agent Workflow

Four logical engineering roles are used.

Agent 4 — Context & DevNotes Sync
→ Agent 2 — Requirements / Gap Analysis
→ Agent 4 — Context & DevNotes Sync
→ Agent 1 — Development
→ Agent 4 — Context & DevNotes Sync
→ Agent 3 — Code Review / Impact / Tests
→ Agent 4 — Context & DevNotes Sync

- Agent 1 implements approved assignment scope.
- Agent 2 protects the assignment boundary.
- Agent 3 reviews actual implementation, impact, concurrency, and tests.
- Agent 4 maintains persistent project context.

All roles may use standard available MCPs/tools when directly useful for repository inspection, files, documentation, testing, or technical references. Tool use must not expand assignment scope. These are logical workflow roles, not independent autonomous agents.

Mandatory verification gates:
1. Static consistency: changed symbols, call sites, DI registrations, package references, configuration, SQL procedure names, and DTO/result shapes must agree across files.
2. Compile gate: execute the real clean build (`docker compose build --no-cache api` for this repository). Static review alone is insufficient.
3. Runtime configuration gate: verify the resolved Compose configuration and ensure the DB healthcheck validates the same application credentials used by the API, not only a root/admin connection.
4. Startup gate: start the required services and verify liveness/readiness.
5. Smoke/API gate: exercise changed endpoints and critical existing flows.
6. Concurrency/assignment gate: run the repository burst/load test and verify expected outcomes, no double booking, and no unexpected 5xx responses.
7. CI gate: the `Compose Runtime Smoke` workflow must pass on the final commit before Agent 3 approval.

If any gate fails, stop deeper review, identify the root cause, send it back for correction, and rerun the failed gate plus the full verification sequence.

Meaningful implementation follows:
PLAN → human review → IMPLEMENT → TEST → human review → COMMIT
