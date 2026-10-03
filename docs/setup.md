# Setup

## Docker first

A clean checkout needs Docker and Git for the normal workflow.

Start with docker compose up --build.

The stack starts MySQL, waits for the database health check, runs database initialization, and then starts the API.

API: http://localhost:8080
Liveness: http://localhost:8080/health/live
Readiness: http://localhost:8080/health/ready
Metrics: http://localhost:8080/metrics

## Database initialization

The repository keeps SQL organized as:
- database/init — create tables
- database/seed — enum and dummy users
- database/migrations — stored procedures

database/00-run-init.sh executes those folders in deterministic order.

MySQL initialization runs only for a new data volume. After changing initialization SQL:
docker compose down -v
docker compose up --build

## Development users

admin / admin123!
user1 / user123!
user2 / user456!
user3 / user789!

These credentials are for local assignment execution only.

## Laptop / Codespace commands

See docs/laptop-setup-commands.md for copy/paste commands.

## Native .NET

If .NET 10 SDK is installed:
dotnet restore
dotnet build
dotnet test

MySQL can remain in Docker:
docker compose up -d db

## Useful Docker commands

docker compose ps
docker compose logs -f api
docker compose logs -f db
docker compose exec db mysql -umyshowbook -pmyshowbook_dev myshowbook
docker compose down
docker compose down -v
