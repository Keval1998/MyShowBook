# Interview Notes

Reservation path:
1. Authenticate and derive user identity from the token.
2. Start one MySQL transaction.
3. Resolve GUIDs to internal IDs.
4. Lock user/show state needed for the booking limit.
5. Lock requested seats in deterministic order.
6. Check idempotency, seat state, ownership, and limit.
7. Write reservation and seat mappings atomically.
8. Commit.
9. Return 201 or a clean 4xx domain outcome.

The key design decision is putting the atomic booking decision at the database boundary instead of using an application-level read-then-write sequence.
