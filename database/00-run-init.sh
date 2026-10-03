#!/bin/bash
set -e

for folder in init seed functions migrations; do
  for file in /docker-entrypoint-initdb.d/$folder/*.sql; do
    [ -f "$file" ] || continue
    echo "Running $file"
    mysql --protocol=socket -uroot -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE" < "$file"
  done
done
