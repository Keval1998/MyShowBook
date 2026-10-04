# UI and Logging Context

## Purpose
The browser surface is intentionally minimal and exists to make assignment verification easier. It does not replace the backend APIs or the database concurrency design.

## Current flow
- Anonymous users can register as customer or admin for local assignment testing.
- Login returns the existing JWT.
- The browser stores the JWT and role in sessionStorage.
- Logout removes both session values.
- Admins can create shows and view recent structured logs.
- Authenticated users can list shows, fetch seat status, and reserve seats.
- The existing owner-only reservation cancellation API remains available.

## Logging
- Serilog.AspNetCore 10.0.0 is used for ASP.NET Core integration.
- Serilog.Sinks.File 7.0.0 writes daily rolling newline-delimited JSON files.
- Console and file use CompactJsonFormatter.
- Logs are retained for 14 files and roll on file size.
- Docker mounts ./logs to /app/logs.
- /logs is admin-only and returns the latest 500 lines.
- X-Correlation-ID is retained through the existing correlation middleware.
- Request completion logs include method, path, status and elapsed time.

## Verification incident
The merged UI/logging change exposed a compile-time defect during the user's clean Docker build: RegistrationHelper called StoredProcedureUtility.Bool(...), but the utility did not define that method. Static review had incorrectly treated the change as verified without an executable build result. The fix adds the missing boolean parameter helper and the review workflow now requires executable build evidence before a review can be marked passed.

## Scope guard
Registration with an admin role is a deliberate local/demo convenience requested for manual assignment testing. It should not be treated as unrestricted production self-service admin registration.
