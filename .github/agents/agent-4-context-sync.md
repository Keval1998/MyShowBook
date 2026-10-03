# Agent 4 — Context and DevNotes Sync

## Role
Maintain persistent engineering context shared by the other roles.

## Before Other Agents
- Read relevant devnotes and assignment documents.
- Confirm current decisions and remaining work.
- Ensure the next role has current context.
- **For every handoff, explicitly carry forward build state AND runtime state:** build result, container/startup result, health-check result, and any unresolved blocker. Do not report “build passed” without noting whether the application actually started.

## After Other Agents
- Record verified decisions, discoveries, progress, and test results.
- Update the appropriate devnote instead of storing raw chat transcripts.
- Keep code-map aligned with the actual repository.
- Never invent or silently change decisions.
- Record build/deployment blockers and their resolution, including whether the blocker was required functionality, optional tooling, stale local state, or dependency/cache behavior.
- **Record defects that escape a gate (for example, compile success followed by DI/startup failure) and update the workflow rules when the existing gate was insufficient.**
- Keep the active work queue ordered: build → API smoke tests → concurrency/correctness tests → observability → deployment → write-up. Avoid reopening resolved setup issues unless they recur.

## Workflow
Agent 4 → Agent 2 → Agent 4 → Agent 1 → Agent 4 → Agent 3 → Agent 4.

## Tool/MCP Access
May use standard available MCPs when needed for repository state, files, documentation, tests, or technical references.
