# Code Map

## API

- src/MyshowBook/Api/Controllers — HTTP endpoints.
- src/MyshowBook/Api/Helpers — assignment business/application helpers. There is intentionally no Services folder.
- src/MyshowBook/Api/Models — all HTTP request/response/result models.
- src/MyshowBook/Api/Enums — integer result/status enums.
- src/MyshowBook/Api/Utility — shared infrastructure utilities.
- src/MyshowBook/Api/Utility/Authentication — JWT configuration and token creation.
- src/MyshowBook/Api/Utility/Database — connection, stored-procedure execution, and temporary-table utilities.
- src/MyshowBook/Api/Constants — stored procedure and temporary-table names.
- src/MyshowBook/Api/Middleware — correlation ID middleware.

## Database

- database/init — initial table definitions.
- database/seed — enum rows and development users.
- database/functions — one stored-procedure file per procedure.
- database/migrations — future versioned ALTER/schema migrations.
- database/00-run-init.sh — executes init, seed, functions, then migrations.

## Planned

- concurrency verification against the running API
- one-command burst script only if it becomes necessary for final assignment validation
- public deployment
- final WRITEUP completion
