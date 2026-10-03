# Agent 1 — Development

## Role
Implement only approved assignment functionality.

## Core rules
- Keep code clean, small, readable, and easy to explain.
- Prefer the simplest design that satisfies the requirement; do not add speculative production architecture.
- Treat the assignment and `docs/paytm-assignment-requirements.md` as the source of truth.
- Keep assignment-specific decisions documented, but write reusable engineering guidance rather than hard-coding one feature's implementation into the agent rules.
- Use minimal comments for non-obvious reasoning only.
- Run the relevant build/tests after every meaningful code change, not only at the end. Compiler/syntax errors are blockers.
- For Docker builds, verify the actual checked-out source/dependency state first; do not diagnose a stale local checkout as a code problem. After dependency changes, use a clean restore/build path.
- Prioritize required assignment functionality. Remove optional tooling/dependencies that block build or deployment when they provide no assignment value.
- Do not spend time on optional developer tooling until the required API builds and runs.

## Code organization
- Keep each model/DTO/record class in its own model file. Do not group unrelated request, response, or result types into a shared `*Models.cs` file.
- Keep models focused on transport/data shape; avoid putting workflow or persistence logic in them.
- Keep shared infrastructure in Utility and feature/application logic in Helpers unless a clearer reviewed structure is needed.
- Prefer small, single-purpose classes and methods over large mixed-responsibility classes.

## Database and SQL
- Prefer MySQL stored procedures for concurrency-critical operations when the assignment benefits from transaction-local atomicity.
- Do not introduce EF Core unless a reviewed requirement gives it a concrete benefit.
- Use parameterized database calls; do not build values directly into SQL.
- Keep multi-row input handling set-based where practical. Temporary tables may be used when a procedure needs a collection of input values, with their lifecycle centralized in one utility.
- Avoid cursors and row-by-row loops inside stored procedures when a set-based operation can express the same behavior.
- Understand the difference between `INSERT`, `UPDATE`, and `DELETE`: `INSERT` creates rows, `UPDATE` changes existing rows selected by its conditions, and `DELETE` removes rows. For concurrency-sensitive state changes, perform the required locking/read validation and the `UPDATE` inside the same transaction.
- Always give state-changing `UPDATE` statements a precise condition identifying the intended rows. For joined or collection-based updates, use a set-based join/update pattern rather than application-side iteration.
- For operations that move state between values, make the allowed state transition explicit and preserve transactional consistency.

## Concurrency and idempotency
- For concurrent reservation-like workflows, reason about the complete transaction: identity serialization where needed, deterministic row-lock order, validation, state change, and commit/rollback.
- Do not rely on a read-then-write sequence without appropriate locking/constraints.
- An idempotency key identifies a logical retry and should be persisted with an appropriate uniqueness constraint.
- A unique idempotency key alone prevents duplicate processing for the same key, but it does not prove that a reused key carries the same request. When request tampering, accidental key reuse, or replay with a different payload matters, persist a deterministic request fingerprint/hash and compare it on reuse.
- Normalize semantically equivalent request data before hashing so the fingerprint represents the logical request rather than irrelevant formatting/order differences.
- Keep idempotency logic scoped to the operation that needs it; do not introduce a generalized idempotency subsystem without a requirement.

## Identifiers and GUIDs
- Prefer non-sequential public identifiers when exposing entity identity externally, while internal numeric keys may remain useful for relationships and locking.
- In .NET, `Guid.ToString("D")` means the standard hyphenated 36-character GUID representation (`8-4-4-4-12`). It is a format specifier, not a special business meaning. Use it when a database/API representation expects the conventional `CHAR(36)` form.
- Keep GUID formatting consistent at system boundaries.

## Transactions and state
- Keep money in integer minor units; never use floating point for monetary values.
- When a state transition must be atomic, keep the relevant read/lock, validation, write, and commit in one transaction.
- For multi-item operations, choose and document all-or-nothing vs best-effort behavior before implementation and make the database operation match that decision.
- Use explicit ownership/identity from authenticated context rather than trusting identity supplied in request bodies.

## Scope discipline
- Do not add UI, background workers, expiry mechanisms, generalized abstractions, or other infrastructure unless the assignment requires them.
- Do not create extra setup/tutorial files when the user can use the conversation as the setup/run reference; keep repository documentation focused on what is needed for the assignment and clean-checkout usage.
- Tool use may include repository, documentation, testing, or technical-reference work when directly useful, but tool use must not expand assignment scope.
