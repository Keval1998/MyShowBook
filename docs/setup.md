# Setup

## Docker first

A clean checkout uses Docker Compose for MySQL and the API.

Set the required environment values in your shell, then run:

docker compose down -v
docker compose up --build

The stack starts MySQL, waits for the database health check, runs database initialization, and then starts the API.

API: http://localhost:8080
Liveness: http://localhost:8080/health/live
Readiness: http://localhost:8080/health/ready
Metrics: http://localhost:8080/metrics

## Database initialization

The repository keeps SQL organized as:

database/init — create tables
database/seed — enum + dummy users
database/functions — one stored procedure per file
database/migrations — future timestamped schema changes

database/00-run-init.sh executes these folders in order.

MySQL initialization runs only for a new data volume. After changing initialization SQL:

docker compose down -v
docker compose up --build

## Configuration

Connection string and JWT signing key are not stored in appsettings.json.

Use environment variables for the connection string and JWT signing key. Do not commit deployment secrets.

ASP.NET Core supports environment variables as configuration providers and maps double underscores to hierarchical configuration keys. citeturn7search1
