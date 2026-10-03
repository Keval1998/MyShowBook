# Testing

## Current review/smoke coverage

The implementation is intended to be tested manually against the running Docker Compose API rather than adding a separate test project at this stage.

### Basic API flow
- health/live returns 200
- health/ready returns 200 when MySQL is reachable
- metrics endpoint returns Prometheus-style output
- admin login returns a JWT
- normal user login returns a JWT
- admin can create a show
- normal user cannot create a show
- show can be read without authentication
- authenticated user can reserve
- authenticated owner can cancel

### Reservation behavior
- single-seat reservation
- multi-seat reservation
- invalid show
- invalid seat
- already confirmed seat
- all-or-nothing multi-seat request
- per-user limit of 4
- same idempotency key + same normalized seat set
- same idempotency key + different seat set
- cancellation by owner
- cancellation by another user
- cancellation replay
- cancelled seat can be reserved again

### Concurrency checks to run later
- many users targeting one hot seat
- overlapping multi-seat requests
- same user firing more than four concurrent seats
- concurrent retries using the same idempotency key
- large burst with approximately 20,000 requests
- zero unexpected 5xx responses
- final available + held + confirmed == total
- metrics available-seat gauge matches GET /shows state

No load-test or test project is being added yet. These checks can be performed against the running service using curl or a temporary external script.
