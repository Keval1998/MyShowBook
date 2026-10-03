# Technical Challenges

Primary challenge: preventing double booking while preserving consistency under concurrent requests.

Planned solution:
- MySQL InnoDB transaction.
- SELECT ... FOR UPDATE.
- Deterministic seat lock ordering.
- Database uniqueness constraints.
- Atomic reservation writes.
- User-row locking where required for MaxSeats.
- Rollback on failed multi-seat reservation.
