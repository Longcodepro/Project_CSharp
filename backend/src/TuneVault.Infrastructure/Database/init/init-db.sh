#!/usr/bin/env bash
set -euo pipefail

DB_NAME="${DB_NAME:-TuneVaultDb}"
SQL_HOST="${SQL_HOST:-db}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASSWORD="${SQL_SERVER_PASSWORD:?SQL_SERVER_PASSWORD is required}"
SCHEMA_FILE="${SCHEMA_FILE:-/scripts/database.sql}"
SEED_FILE="${SEED_FILE:-/scripts/seed.sql}"
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"

if [[ ! -x "$SQLCMD" ]]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
fi

if [[ ! -x "$SQLCMD" ]]; then
  echo "sqlcmd was not found in the container image." >&2
  exit 1
fi

echo "Waiting for SQL Server at ${SQL_HOST}..."
until "$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; do
  sleep 2
done

echo "Ensuring database ${DB_NAME} exists..."
"$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d master -b -Q "IF DB_ID(N'${DB_NAME}') IS NULL CREATE DATABASE [${DB_NAME}];"

schema_exists="$("$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d "$DB_NAME" -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID(N'dbo.Users', N'U') IS NULL THEN 0 ELSE 1 END;" | tr -d '\r' | tail -n 1)"
if [[ "$schema_exists" != "1" ]]; then
  echo "Applying schema snapshot from ${SCHEMA_FILE}..."
  "$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d "$DB_NAME" -b -i "$SCHEMA_FILE"
else
  echo "Schema already exists; skipping schema snapshot."
fi

seed_exists="$("$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d "$DB_NAME" -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Id = 'U003') THEN 1 ELSE 0 END;" | tr -d '\r' | tail -n 1)"
if [[ "$seed_exists" != "1" ]]; then
  echo "Applying seed data from ${SEED_FILE}..."
  "$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d "$DB_NAME" -b -i "$SEED_FILE"
else
  echo "Seed already present; skipping seed file."
fi

collection_likes_exists="$("$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d "$DB_NAME" -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID(N'dbo.CollectionLikes', N'U') IS NULL THEN 0 ELSE 1 END;" | tr -d '\r' | tail -n 1)"
if [[ "$collection_likes_exists" != "1" ]]; then
  echo "Applying CollectionLikes fix from /scripts/init/V14_EnsureCollectionLikes.sql..."
  "$SQLCMD" -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -C -d "$DB_NAME" -b -i /scripts/init/V14_EnsureCollectionLikes.sql
else
  echo "CollectionLikes table already exists; skipping fix script."
fi

echo "TuneVaultDb initialization complete."
