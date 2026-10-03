# Technical Challenges

- Hot-seat contention: the atomic decision must occur at the DB boundary; read-then-write is unsafe.
- Multi-seat locking: sort internal seat IDs before SELECT ... FOR UPDATE to establish deterministic lock order.
- Per-user limit: enforce inside the transaction while protecting concurrent requests for the same user/show.
- Idempotency: persist the key so process restarts and multiple instances cannot lose the correctness boundary.
- Reconciliation: derive show counts from seat state and maintain available + held + confirmed == total.
- Deployability: Docker Compose should make a clean checkout runnable with one command.
