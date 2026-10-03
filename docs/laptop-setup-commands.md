# Laptop / Codespace Setup Commands

## 1. Get the latest code

```bash
git pull origin main
```

## 2. Start API + MySQL

Docker is the primary setup path. No local MySQL installation is required.

```bash
docker compose down
docker compose up --build
```

The first startup creates tables, enum rows, dummy users, and stored procedures automatically. MySQL initialization runs only for a new data volume. Reset it after changing initialization SQL:

```bash
docker compose down -v
docker compose up --build
```

## 3. Verify

In another terminal:

```bash
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
curl http://localhost:8080/metrics
```

## 4. Get a token

Admin:

```bash
curl -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123!"}'
```

User:

```bash
curl -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"user123!"}'
```

Then:

```bash
export TOKEN='PASTE_ACCESS_TOKEN_HERE'
```

## 5. Create a show

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

## 6. Reserve

Use a normal user token:

```bash
curl -X POST "http://localhost:8080/shows/$SHOW_ID/reserve" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"seats":["A1"],"idempotency_key":"demo-1"}'
```

## 7. Read show state

```bash
curl "http://localhost:8080/shows/$SHOW_ID"
```

## 8. Cancel

```bash
export RESERVATION_ID='PASTE_RESERVATION_GUID_HERE'

curl -X POST "http://localhost:8080/reservations/$RESERVATION_ID/cancel" \
  -H "Authorization: Bearer $TOKEN"
```

## 9. Native .NET

If .NET 10 SDK is installed:

```bash
dotnet restore
dotnet build
dotnet test
```

MySQL can remain in Compose:

```bash
docker compose up -d db
```

## 10. Useful Docker commands

```bash
docker compose ps
docker compose logs -f api
docker compose logs -f db
docker compose exec db mysql -umyshowbook -pmyshowbook_dev myshowbook
docker compose down
docker compose down -v
```
