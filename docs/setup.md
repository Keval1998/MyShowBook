# Local Setup

## Prerequisites
- .NET 10 SDK
- Docker Desktop / Docker Engine with Compose
- Git
- Python 3.11+ for the concurrency test

## Restore and Build
```bash
dotnet restore
dotnet build
```

## Run the API
```bash
dotnet run --project src/MyshowBook/Api
```

## Database
The application uses MySQL 8 with InnoDB. The final Docker Compose setup will provide MySQL locally.

Database setup will be:
1. Start MySQL.
2. Apply schema.
3. Seed status/reference data.
4. Apply stored procedures.
5. Start the API.

Exact commands will be finalized with the database milestone.

## Tests
```bash
dotnet test
```

## Concurrency Test
The high-concurrency test will run from load-tests/ after the reservation API is available. It will verify the booking invariant, not merely count HTTP successes.

## Configuration
Do not commit secrets or local credentials. Use local development configuration or environment variables.

## Docker
The final setup will support:
```bash
docker compose up --build
```
Exact services, ports, credentials, and health checks will be documented when Docker configuration is added.
