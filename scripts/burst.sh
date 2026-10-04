#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:-http://localhost:8080}"
COUNT="${BURST_COUNT:-20000}"
ADMIN_USERNAME="${ADMIN_USERNAME:-admin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-admin123!}"
USER_USERNAME="${USER_USERNAME:-user1}"
USER_PASSWORD="${USER_PASSWORD:-user123!}"

tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT

login() {
  curl -fsS -X POST "$BASE_URL/auth/token"     -H 'Content-Type: application/json'     -d "{\"username\":\"$1\",\"password\":\"$2\"}" |
    jq -r '.access_token'
}

ADMIN_TOKEN="$(login "$ADMIN_USERNAME" "$ADMIN_PASSWORD")"
USER_TOKEN="$(login "$USER_USERNAME" "$USER_PASSWORD")"

SHOW="$(curl -fsS -X POST "$BASE_URL/shows"   -H "Authorization: Bearer $ADMIN_TOKEN"   -H 'Content-Type: application/json'   -d '{"name":"burst-test","seats":["HOT1","HOT2","HOT3"],"price_paise":25000}' |
  jq -r '.show_guid')"

echo "Show: $SHOW"
echo "Running $COUNT concurrent attempts for HOT1..."

seq 1 "$COUNT" |
  xargs -P 200 -I{} sh -c \
    'curl -sS -o /dev/null --connect-timeout 10 --max-time 90 -w "%{http_code}\n" \
      -H "Authorization: Bearer '"$USER_TOKEN"'" \
      -H "Content-Type: application/json" \
      -X POST "'"$BASE_URL"'/shows/'"$SHOW"'/reserve" \
      -d "{\"seats\":[\"HOT1\"],\"idempotency_key\":\"burst-{}\"}"' |
  sort | uniq -c | tee "$tmp/outcomes"

winner="$(awk '$2 == 201 {print $1+0}' "$tmp/outcomes")"
conflict="$(awk '$2 == 409 {print $1+0}' "$tmp/outcomes")"
errors="$(awk '$2 != 201 && $2 != 409 {sum += $1} END {print sum+0}' "$tmp/outcomes")"

echo "201 confirmed: ${winner:-0}"
echo "409 declined:  ${conflict:-0}"
echo "other/5xx:     $errors"

[ "${winner:-0}" = "1" ] || { echo "FAIL: expected exactly one winner"; exit 1; }
[ "${conflict:-0}" = "$((COUNT - 1))" ] || { echo "FAIL: expected $((COUNT - 1)) conflicts"; exit 1; }
[ "$errors" = "0" ] || { echo "FAIL: unexpected HTTP outcomes"; exit 1; }

curl -fsS "$BASE_URL/shows/$SHOW" | jq -e '
  (.available_seats + .held_seats + .confirmed_seats == .total_seats)
  and .confirmed_seats == 1
  and .available_seats == 2
  and .held_seats == 0
' >/dev/null

echo "PASS: hot-seat burst and final reconciliation"
