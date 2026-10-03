# Laptop / Codespace Setup Commands

## 1. Pull the latest code

```bash
git pull origin main
```

## 2. Set local environment values

The repository intentionally does not store the database connection string or JWT signing key in appsettings.

Copy this block into a Bash terminal:

```bash
export MYSQL_DATABASE="myshowbook"
export MYSQL_USER="myshowbook"
export MYSQL_PASSWORD="myshowbook_dev"
export MYSQL_ROOT_PASSWORD="root_dev"

export CONNECTIONSTRINGS__DEFAULT="Server=db;Port=3306;Database=$MYSQL_DATABASE;User=$MYSQL_USER;Password=$MYSQL_PASSWORD;"
export JWT_KEY="local-development-jwt-key-change-this-value-1234567890"
export JWT_ISSUER="MyShowBook"
export JWT_AUDIENCE="MyShowBook"
export JWT_EXPIRY_MINUTES="60"
```

ASP.NET Core reads hierarchical environment variables using double underscores, so CONNECTIONSTRINGS__DEFAULT becomes ConnectionStrings:Default and JWT_KEY is passed into Jwt:Key by Compose. Environment variables override appsettings values. citeturn7search1turn7search2

## 3. Start API + MySQL

```bash
docker compose down -v
docker compose up --build
```

The first startup creates tables, enum rows, dummy users, and stored procedures.

MySQL initialization is only performed for a new database volume. Use down -v again after changing initialization SQL.

## 4. Verify

Open another terminal with the same environment exports:

```bash
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
curl http://localhost:8080/metrics
```

## 5. Get a token

Admin:

```bash
curl -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123!"}'
```

Normal user:

```bash
curl -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"user123!"}'
```

Then:

```bash
export TOKEN='PASTE_ACCESS_TOKEN_HERE'
```

## 6. Create a show

Use the admin token:

```bash
curl -X POST http://localhost:8080/shows \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"friday-night","seats":["A1","A2","A3","A4"],"price_paise":25000}'
```

Copy the returned show_guid:

```bash
export SHOW_ID='PASTE_SHOW_GUID_HERE'
```

## 7. Reserve

Use a normal user token:

```bash
curl -X POST "http://localhost:8080/shows/$SHOW_ID/reserve" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"seats":["A1"],"idempotency_key":"demo-1"}'
```

## 8. Read show state

```bash
curl "http://localhost:8080/shows/$SHOW_ID"
```

## 9. Cancel

```bash
export RESERVATION_ID='PASTE_RESERVATION_GUID_HERE'

curl -X POST "http://localhost:8080/reservations/$RESERVATION_ID/cancel" \
  -H "Authorization: Bearer $TOKEN"
```

## 10. Native .NET

If .NET 10 SDK is installed, stop the API container and use the same environment values with the native process.

Change the connection string host from db to localhost:

```bash
export CONNECTIONSTRINGS__DEFAULT="Server=localhost;Port=3306;Database=$MYSQL_DATABASE;User=$MYSQL_USER;Password=$MYSQL_PASSWORD;"
dotnet restore
dotnet build
dotnet run --project src/MyshowBook/Api
```

MySQL can remain in Docker:

```bash
docker compose up -d db
```

## 11. Useful Docker commands

```bash
docker compose ps
docker compose logs -f api
docker compose logs -f db
docker compose exec db mysql -umyshowbook -pmyshowbook_dev myshowbook
docker compose down
docker compose down -v
```
