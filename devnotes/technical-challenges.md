# Technical Challenges

- Hot-seat contention: the atomic decision must occur at the DB boundary; read-then-write is unsafe.
- Multi-seat locking: sort internal seat IDs before SELECT ... FOR UPDATE to establish deterministic lock order.
- Per-user limit: enforce inside the transaction while protecting concurrent requests for the same user/show.
- Idempotency: persist the key so process restarts and multiple instances cannot lose the correctness boundary.
- Reconciliation: derive show counts from seat state and maintain available + held + confirmed == total.
- Deployability: Docker Compose should make a clean checkout runnable with one command.
- Runtime validation: a successful compile does not validate ASP.NET Core dependency injection. A startup failure exposed a missing direct DI registration for JwtOptions consumed by JwtTokenService. The workflow now requires constructor-to-registration checks plus actual container startup and health verification before handoff.

- QA review: executable GitHub Actions validation was prepared on a disposable branch, but no workflow run was created by the repository, so no dynamic test result is being claimed. Static review identified stale setup documentation and the missing required burst script; both were corrected in the QA fix branch.
- Concurrency hardening: reservation and cancellation database calls now retry transient MySQL deadlock (1213) and lock-wait timeout (1205) failures up to three attempts. The reservation path is idempotent, so a retry after a committed transaction can safely return the original reservation.
