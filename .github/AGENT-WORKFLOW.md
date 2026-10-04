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

All roles may use standard available MCPs/tools when directly useful for repository inspection, files, documentation, testing, or technical references. Tool use must not expand assignment scope.

Mandatory verification gates:
1. Static consistency: changed symbols, call sites, DI registrations, package references, configuration, SQL procedure names, and DTO/result shapes must agree across files.
2. Compile gate: execute the real clean build (`docker compose build --no-cache api` for this repository). Static review alone is insufficient.
3. Startup gate: start the required services and verify liveness/readiness.
4. Smoke/API gate: exercise changed endpoints and critical existing flows.
5. Only after all gates pass may Agent 3 approve the change.

If any gate fails, stop deeper review, identify the root cause, send it back for correction, and rerun the failed gate plus the full verification sequence.

Meaningful implementation follows:
PLAN → human review → IMPLEMENT → TEST → human review → COMMIT
