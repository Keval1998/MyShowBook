#!/bin/bash
set -e

for file in /docker-entrypoint-initdb.d/init/*.sql /docker-entrypoint-initdb.d/seed/*.sql /docker-entrypoint-initdb.d/migrations/*.sql; do
  [ -f "$file" ] || continue
  echo "Running $file"
  mysql --protocol=socket -uroot -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE" < "$file"
done
