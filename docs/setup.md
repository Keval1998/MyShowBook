# Setup

## Recommended: Docker

A clean checkout should require only Docker and Git for the normal path.

Start the complete stack:

```bash
docker compose up --build
```

The Compose setup builds the API image, starts MySQL 8, waits for the database health check, and starts the API.

API: http://localhost:8080

Liveness: http://localhost:8080/health/live

Check status:

```bash
docker compose ps
```

View API logs:

```bash
docker compose logs -f api
```

Stop:

```bash
docker compose down
```

Reset local database state:

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

## Database Initialization

Database schema, seed data, and stored procedures will be added under `database/`. Compose will run them automatically when the database is initialized.

## Burst Test

Once the reservation API is available:

```bash
./load-tests/burst.sh http://localhost:8080
```

The final command will report confirmed reservations, decline reasons, unexpected 5xx responses, and final reconciliation.

## Clean Checkout

The repository must build and run from a clean clone without undocumented manual database setup.

## Configuration

Do not commit secrets or local credentials. Development values belong in local configuration/environment variables; deployment secrets belong in the hosting platform.
