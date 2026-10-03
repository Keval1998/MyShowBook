# Setup

## Recommended: Docker

A clean checkout should require only Docker and Git for the normal path.

```bash
docker compose up --build
```

This will build the API, start MySQL, wait for the database health check, initialize the database, and start the API. Check status with `docker compose ps` and logs with `docker compose logs -f api`.

Stop with:

```bash
docker compose down
```

Reset local database state with:

```bash
docker compose down -v
docker compose up --build
```

## Native Development

Docker is the primary evaluator path. Native development can use the .NET 10 SDK:

```bash
dotnet restore
dotnet build
dotnet test
```

Do not install MySQL manually for the normal workflow; Compose provides it.

## Burst Test

Once the stack is running, the final README will provide the exact one-command burst invocation and default API port. The burst reports confirmed reservations, decline reasons, unexpected 5xx responses, and final reconciliation.

## Clean Checkout

The repository must build and run from a clean clone without undocumented manual database setup.

## Configuration

Do not commit secrets or local credentials. Development values belong in local configuration/environment variables; deployment secrets belong in the hosting platform.
