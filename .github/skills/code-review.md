# Skill — Code Review and Impact

Review actual changes against requirements and devnotes. Prioritize transaction boundaries, locking, constraints, concurrency, rollback, idempotency, and test evidence.

## Verification rule

Static inspection can identify likely defects but cannot establish compilation or runtime correctness. For every meaningful C# change:
- run the relevant `dotnet build` or `dotnet publish`;
- verify changed symbols/call signatures against their actual definitions;
- for Dockerized application changes, run the container/startup and health check when execution is available;
- report the exact verification status and do not call an unexecuted gate passed.

Do not introduce unrelated improvements.
