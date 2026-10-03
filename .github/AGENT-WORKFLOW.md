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

Meaningful implementation follows:
PLAN → human review → IMPLEMENT → TEST → human review → COMMIT
