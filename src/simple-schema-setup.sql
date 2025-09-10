-- ============================================================================
-- eShop Database Schema Creation - Simple Version
-- ============================================================================
-- Run these commands one by one in your PostgreSQL client (pgAdmin, psql, etc.)

-- 1. Connect to your eshopdb database first

-- 2. Create all schemas
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS basket;
CREATE SCHEMA IF NOT EXISTS ordering;
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS shared;
CREATE SCHEMA IF NOT EXISTS messaging;

-- 3. Create user if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'eshopuser') THEN
        CREATE USER eshopuser WITH ENCRYPTED PASSWORD 'EShop123!';
    END IF;
END
$$;

-- 4. Grant schema usage permissions
GRANT USAGE ON SCHEMA catalog TO eshopuser;
GRANT USAGE ON SCHEMA basket TO eshopuser;
GRANT USAGE ON SCHEMA ordering TO eshopuser;
GRANT USAGE ON SCHEMA identity TO eshopuser;
GRANT USAGE ON SCHEMA shared TO eshopuser;
GRANT USAGE ON SCHEMA messaging TO eshopuser;
GRANT USAGE ON SCHEMA public TO eshopuser;

-- 5. Grant create permissions on schemas
GRANT CREATE ON SCHEMA catalog TO eshopuser;
GRANT CREATE ON SCHEMA basket TO eshopuser;
GRANT CREATE ON SCHEMA ordering TO eshopuser;
GRANT CREATE ON SCHEMA identity TO eshopuser;
GRANT CREATE ON SCHEMA shared TO eshopuser;
GRANT CREATE ON SCHEMA messaging TO eshopuser;
GRANT CREATE ON SCHEMA public TO eshopuser;

-- 6. Grant permissions on existing tables and sequences
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA identity TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO eshopuser;

GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA identity TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO eshopuser;

-- 7. Grant default privileges for future objects
ALTER DEFAULT PRIVILEGES IN SCHEMA catalog GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA basket GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA ordering GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA identity GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA shared GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON TABLES TO eshopuser;

ALTER DEFAULT PRIVILEGES IN SCHEMA catalog GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA basket GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA ordering GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA identity GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA shared GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON SEQUENCES TO eshopuser;

-- 8. Create useful extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- 9. Verify schemas were created
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name IN ('catalog', 'basket', 'ordering', 'identity', 'shared', 'messaging')
ORDER BY schema_name;