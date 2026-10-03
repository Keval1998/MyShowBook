# Code Map

## API
- src/MyshowBook/Api/Controllers — HTTP endpoints.
- src/MyshowBook/Api/Services — application/database orchestration.
- src/MyshowBook/Api/Database — MySQL connection, stored procedure, and temp-table helpers.
- src/MyshowBook/Api/Authentication — minimal JWT token handling.
- src/MyshowBook/Api/Models — request/response contracts.
- src/MyshowBook/Api/Constants — stored procedure and temp-table names.
- src/MyshowBook/Api/Enums — database status/metric enums.
- src/MyshowBook/Api/Helpers — hashing, identity, and reader helpers.
- src/MyshowBook/Api/Middleware — correlation IDs.

## Database
- database/init — table creation.
- database/seed — enum and development user data.
- database/migrations — stored procedures.
- database/00-run-init.sh — executes the three database folders in order during first MySQL initialization.

## Planned
- tests — automated correctness/concurrency tests.
- load-tests — one-command high-concurrency burst.
- final deployment and WRITEUP completion.
