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

Meaningful implementation follows:
PLAN → human review → IMPLEMENT → TEST → human review → COMMIT.

## Hard verification gates

A review may not be reported as passed from static inspection alone.

1. **Compile gate:** execute the relevant `dotnet build` or `dotnet publish` against the current checkout. Record the exact command and result.
2. **Startup gate:** for application/container changes, start the actual service and verify it remains running and its health/liveness endpoint responds.
3. **API smoke gate:** exercise the changed/critical API path when practical.
4. **Assignment gate:** after executable gates pass, review concurrency, idempotency, authentication, observability, Docker, and required deliverables.

If execution is unavailable, the status must be explicitly **UNVERIFIED**. Never use words such as “verified”, “passes”, or “ready” for a build/runtime claim without command evidence.

If a gate fails, fix the root cause and repeat the failed gate before proceeding.