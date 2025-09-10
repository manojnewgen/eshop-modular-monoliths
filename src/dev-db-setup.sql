-- Development Database Setup for eShop Modular Monolith
-- This creates eshopdb_dev and all required schemas

-- Connect as superuser and create development database
SELECT 'CREATE DATABASE eshopdb_dev' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'eshopdb_dev');
\gexec

-- Connect to the development database
\c eshopdb_dev;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create schemas for all modules
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS basket;
CREATE SCHEMA IF NOT EXISTS ordering;
CREATE SCHEMA IF NOT EXISTS keycloak_dev;  -- Keycloak development schema
CREATE SCHEMA IF NOT EXISTS shared;
CREATE SCHEMA IF NOT EXISTS messaging;

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
GRANT USAGE ON SCHEMA keycloak_dev TO eshopuser;
GRANT USAGE ON SCHEMA shared TO eshopuser;
GRANT USAGE ON SCHEMA messaging TO eshopuser;
GRANT USAGE ON SCHEMA public TO eshopuser;

-- Grant all privileges on schemas
GRANT ALL PRIVILEGES ON SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA keycloak_dev TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON SCHEMA public TO eshopuser;

-- Grant table privileges (for future tables)
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA keycloak_dev TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO eshopuser;

-- Grant sequence privileges (for future sequences)
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA catalog TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA basket TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA ordering TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA keycloak_dev TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA shared TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA messaging TO eshopuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO eshopuser;

-- Grant default privileges for future objects
ALTER DEFAULT PRIVILEGES IN SCHEMA catalog GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA basket GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA ordering GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak_dev GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA shared GRANT ALL ON TABLES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON TABLES TO eshopuser;

ALTER DEFAULT PRIVILEGES IN SCHEMA catalog GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA basket GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA ordering GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak_dev GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA shared GRANT ALL ON SEQUENCES TO eshopuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON SEQUENCES TO eshopuser;

-- Display created schemas
SELECT schema_name, schema_owner FROM information_schema.schemata 
WHERE schema_name IN ('catalog', 'basket', 'ordering', 'keycloak_dev', 'shared', 'messaging')
ORDER BY schema_name;

-- Success message
\echo 'Development database setup completed successfully!'
\echo 'Database: eshopdb_dev'
\echo 'Schemas: catalog, basket, ordering, keycloak_dev, shared, messaging'
