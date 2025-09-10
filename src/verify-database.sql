-- SQL Script to check database schemas and tables
-- Run this in any PostgreSQL client (pgAdmin, DBeaver, etc.)

-- 1. Check current database
SELECT current_database() as current_db, current_user as current_user;

-- 2. List all custom schemas
SELECT schema_name, schema_owner
FROM information_schema.schemata 
WHERE schema_name IN ('catalog', 'basket', 'ordering', 'shared', 'messaging', 'identity')
ORDER BY schema_name;

-- 3. Count tables per schema
SELECT 
    schemaname as schema_name,
    COUNT(*) as table_count,
    string_agg(tablename, ', ' ORDER BY tablename) as tables
FROM pg_tables 
WHERE schemaname IN ('catalog', 'basket', 'ordering', 'shared', 'messaging', 'identity', 'public')
GROUP BY schemaname
ORDER BY schemaname;

-- 4. Check migration history tables
SELECT 
    schemaname || '.__EFMigrationsHistory' as migration_table,
    COUNT(*) as migration_count
FROM pg_tables 
WHERE tablename = '__EFMigrationsHistory'
GROUP BY schemaname
ORDER BY schemaname;

-- 5. List all tables with their schemas
SELECT 
    schemaname as schema_name,
    tablename as table_name,
    tableowner as table_owner
FROM pg_tables 
WHERE schemaname IN ('catalog', 'basket', 'ordering', 'shared', 'messaging', 'identity')
ORDER BY schemaname, tablename;