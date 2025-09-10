-- ============================================================================
-- eShop Modular Monolith Database Schema Creation Script
-- ============================================================================
-- This script creates schemas for all modules and Keycloak
-- Run this in your local PostgreSQL as superuser (postgres)

-- Connect to the eshopdb database
\c eshopdb;

-- ============================================================================
-- 1. CREATE SCHEMAS
-- ============================================================================

-- Module Schemas
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS basket;
CREATE SCHEMA IF NOT EXISTS ordering;
CREATE SCHEMA IF NOT EXISTS identity;  -- For Keycloak
CREATE SCHEMA IF NOT EXISTS shared;    -- For shared/audit tables
CREATE SCHEMA IF NOT EXISTS messaging; -- For message/event tables

-- ============================================================================
-- 2. CREATE USERS AND GRANT PERMISSIONS
-- ============================================================================

-- Ensure eshopuser exists
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'eshopuser') THEN
        CREATE USER eshopuser WITH ENCRYPTED PASSWORD 'EShop123!';
    END IF;
END
$$;

-- Grant schema permissions to eshopuser
GRANT USAGE ON SCHEMA catalog TO eshopuser;
GRANT USAGE ON SCHEMA basket TO eshopuser;
GRANT USAGE ON SCHEMA ordering TO eshopuser;
GRANT USAGE ON SCHEMA identity TO eshopuser;
GRANT USAGE ON SCHEMA shared TO eshopuser;
GRANT USAGE ON SCHEMA messaging TO eshopuser;
GRANT USAGE ON SCHEMA public TO eshopuser;

-- Grant all privileges on schemas
GRANT ALL PRIVILEGES ON SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA identity TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA public TO eshopuser;

-- Grant table privileges (for future tables)
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA identity TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO eshopuser;

-- Grant sequence privileges (for future sequences)
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA identity TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO eshopuser;

-- Grant default privileges for future objects
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

-- ============================================================================
-- 3. CREATE EXTENSIONS (if needed)
-- ============================================================================

-- Create extensions that might be needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";  -- For text search

-- ============================================================================
-- 4. SCHEMA INFORMATION
-- ============================================================================

-- Display created schemas with descriptions
SELECT 
    schema_name,
    schema_owner,
    'Module schema for ' || 
    CASE 
        WHEN schema_name = 'catalog' THEN 'Product Catalog Management'
        WHEN schema_name = 'basket' THEN 'Shopping Cart Management'
        WHEN schema_name = 'ordering' THEN 'Order Processing'
        WHEN schema_name = 'identity' THEN 'Keycloak Identity Server'
        WHEN schema_name = 'shared' THEN 'Shared/Audit Tables'
        WHEN schema_name = 'messaging' THEN 'Event/Message Tables'
        ELSE 'General Purpose'
    END as description
FROM information_schema.schemata 
WHERE schema_name IN ('catalog', 'basket', 'ordering', 'identity', 'shared', 'messaging')
ORDER BY schema_name;

-- Show schema permissions for eshopuser using pg_namespace and pg_roles
SELECT 
    n.nspname AS schema_name,
    r.rolname AS role_name,
    CASE 
        WHEN has_schema_privilege('eshopuser', n.nspname, 'USAGE') THEN 'USAGE' 
        ELSE ''
    END ||
    CASE 
        WHEN has_schema_privilege('eshopuser', n.nspname, 'CREATE') THEN ', CREATE' 
        ELSE ''
    END AS privileges
FROM pg_namespace n
CROSS JOIN pg_roles r
WHERE n.nspname IN ('catalog', 'basket', 'ordering', 'identity', 'shared', 'messaging', 'public')
  AND r.rolname = 'eshopuser'
ORDER BY n.nspname;

-- Verify extensions are installed
SELECT 
    extname AS extension_name,
    extversion AS version,
    n.nspname AS schema_name
FROM pg_extension e
JOIN pg_namespace n ON n.oid = e.extnamespace
WHERE extname IN ('uuid-ossp', 'pg_trgm')
ORDER BY extname;

-- Success messages using RAISE NOTICE (instead of PRINT which doesn't exist in PostgreSQL)
DO $$
BEGIN
    RAISE NOTICE 'Database schemas created successfully!';
    RAISE NOTICE 'Schemas available:';
    RAISE NOTICE '  - catalog: Product catalog tables';
    RAISE NOTICE '  - basket: Shopping cart tables';
    RAISE NOTICE '  - ordering: Order processing tables';
    RAISE NOTICE '  - identity: Keycloak identity tables';
    RAISE NOTICE '  - shared: Shared/audit tables';
    RAISE NOTICE '  - messaging: Event/message tables';
    RAISE NOTICE '';
    RAISE NOTICE 'Extensions installed: uuid-ossp, pg_trgm';
    RAISE NOTICE 'User "eshopuser" has been granted permissions on all schemas.';
END
$$;