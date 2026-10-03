# Interview Notes

Reservation explanation:
1. Authenticate the user.
2. Start one DB transaction.
3. Resolve GUIDs and lock required rows.
4. Lock requested seats in deterministic order.
5. Check availability and MaxSeats.
6. Persist reservation and seat mappings.
7. Commit.
8. Return reservation.

Key trade-off: stored procedures keep concurrency-critical work close to MySQL locking semantics without introducing an ORM or unnecessary abstraction.
