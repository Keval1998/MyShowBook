# Database Migrations

Stored procedures live in database/functions/, one file per procedure.

Schema changes that alter existing tables belong here. Do not edit an already-applied migration.

Use this format:

2026-10-03-15-01-add-xyz-column-to-table.sql

Each migration should contain only the change required for that version.
