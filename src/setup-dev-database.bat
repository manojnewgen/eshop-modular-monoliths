@echo off
echo ??? Development Database Schema Setup
echo =====================================

echo.
echo ?? Checking if PostgreSQL container is running...
docker ps --filter "name=eshop-database" --format "table {{.Names}}\t{{.Status}}" | findstr "eshop-database"
if %ERRORLEVEL% NEQ 0 (
    echo ? PostgreSQL container is not running
    echo Please start your Docker services first using:
    echo   start-dev-services.bat
    pause
    exit /b 1
)
echo ? PostgreSQL container is running

echo.
echo ?? Waiting for PostgreSQL to be ready...
timeout /t 5 /nobreak > nul

echo.
echo ??? Setting up DEVELOPMENT database schemas...
echo This will create the required schemas for the eShop development environment

echo.
echo ?? What this script will do:
echo   • Create development database: eshopdb_dev
echo   • Create schemas: catalog, basket, ordering, keycloak_dev, shared, messaging
echo   • Set up proper permissions for eshopuser
echo   • Install required PostgreSQL extensions

echo.
echo ?? NOTE: This is specifically for DEVELOPMENT environment
echo Database: eshopdb_dev (separate from production)
echo Keycloak Schema: keycloak_dev

echo.
set /p confirm="Do you want to proceed with development setup? (Y/N): "
if /i "%confirm%" NEQ "Y" (
    echo Operation cancelled.
    pause
    exit /b 0
)

echo.
echo ?? Creating development database and schemas...

:: Create the development database setup SQL
echo Creating development setup script...
(
echo -- Development Database Setup for eShop Modular Monolith
echo -- This creates eshopdb_dev and all required schemas
echo.
echo -- Connect as superuser and create development database
echo SELECT 'CREATE DATABASE eshopdb_dev' WHERE NOT EXISTS ^(SELECT FROM pg_database WHERE datname = 'eshopdb_dev'^);
echo \gexec
echo.
echo -- Connect to the development database
echo \c eshopdb_dev;
echo.
echo -- Create extensions
echo CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
echo CREATE EXTENSION IF NOT EXISTS "pgcrypto";
echo CREATE EXTENSION IF NOT EXISTS "pg_trgm";
echo.
echo -- Create schemas for all modules
echo CREATE SCHEMA IF NOT EXISTS catalog;
echo CREATE SCHEMA IF NOT EXISTS basket;
echo CREATE SCHEMA IF NOT EXISTS ordering;
echo CREATE SCHEMA IF NOT EXISTS keycloak_dev;  -- Keycloak development schema
echo CREATE SCHEMA IF NOT EXISTS shared;
echo CREATE SCHEMA IF NOT EXISTS messaging;
echo.
echo -- Ensure eshopuser exists
echo DO $$
echo BEGIN
echo     IF NOT EXISTS ^(SELECT 1 FROM pg_roles WHERE rolname = 'eshopuser'^) THEN
echo         CREATE USER eshopuser WITH ENCRYPTED PASSWORD 'EShop123!';
echo     END IF;
echo END
echo $$;
echo.
echo -- Grant schema permissions to eshopuser
echo GRANT USAGE ON SCHEMA catalog TO eshopuser;
echo GRANT USAGE ON SCHEMA basket TO eshopuser;
echo GRANT USAGE ON SCHEMA ordering TO eshopuser;
echo GRANT USAGE ON SCHEMA keycloak_dev TO eshopuser;
echo GRANT USAGE ON SCHEMA shared TO eshopuser;
echo GRANT USAGE ON SCHEMA messaging TO eshopuser;
echo GRANT USAGE ON SCHEMA public TO eshopuser;
echo.
echo -- Grant all privileges on schemas
echo GRANT ALL PRIVILEGES ON SCHEMA catalog TO eshopuser;
echo GRANT ALL PRIVILEGES ON SCHEMA basket TO eshopuser;
echo GRANT ALL PRIVILEGES ON SCHEMA ordering TO eshopuser;
echo GRANT ALL PRIVILEGES ON SCHEMA keycloak_dev TO eshopuser;
echo GRANT ALL PRIVILEGES ON SCHEMA shared TO eshopuser;
echo GRANT ALL PRIVILEGES ON SCHEMA messaging TO eshopuser;
echo GRANT ALL PRIVILEGES ON SCHEMA public TO eshopuser;
echo.
echo -- Grant table privileges ^(for future tables^)
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA catalog TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA basket TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA ordering TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA keycloak_dev TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA shared TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA messaging TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO eshopuser;
echo.
echo -- Grant sequence privileges ^(for future sequences^)
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA catalog TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA basket TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA ordering TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA keycloak_dev TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA shared TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA messaging TO eshopuser;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO eshopuser;
echo.
echo -- Grant default privileges for future objects
echo ALTER DEFAULT PRIVILEGES IN SCHEMA catalog GRANT ALL ON TABLES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA basket GRANT ALL ON TABLES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA ordering GRANT ALL ON TABLES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak_dev GRANT ALL ON TABLES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA shared GRANT ALL ON TABLES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON TABLES TO eshopuser;
echo.
echo ALTER DEFAULT PRIVILEGES IN SCHEMA catalog GRANT ALL ON SEQUENCES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA basket GRANT ALL ON SEQUENCES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA ordering GRANT ALL ON SEQUENCES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak_dev GRANT ALL ON SEQUENCES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA shared GRANT ALL ON SEQUENCES TO eshopuser;
echo ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON SEQUENCES TO eshopuser;
echo.
echo -- Display created schemas
echo SELECT schema_name, schema_owner FROM information_schema.schemata 
echo WHERE schema_name IN ^('catalog', 'basket', 'ordering', 'keycloak_dev', 'shared', 'messaging'^)
echo ORDER BY schema_name;
echo.
echo -- Success message
echo \echo 'Development database setup completed successfully!'
echo \echo 'Database: eshopdb_dev'
echo \echo 'Schemas: catalog, basket, ordering, keycloak_dev, shared, messaging'
) > dev-db-setup.sql

:: Copy and execute the setup script
docker cp dev-db-setup.sql eshop-database:/tmp/dev-db-setup.sql

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to copy SQL file to container
    pause
    exit /b 1
)

:: Execute as postgres superuser to create database and schemas
docker exec -i eshop-database psql -U postgres -f /tmp/dev-db-setup.sql

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to execute development database setup script
    echo.
    echo ?? Troubleshooting tips:
    echo   1. Ensure PostgreSQL is fully started
    echo   2. Check if postgres superuser is available
    echo   3. Verify the container is healthy: docker-compose ps
    pause
    exit /b 1
)

echo.
echo ? Development database setup completed successfully!

echo.
echo ??? Verifying development schema creation...
docker exec -i eshop-database psql -U eshopuser -d eshopdb_dev -c "\dn"

echo.
echo ?? Now restarting Keycloak to pick up the new schema...
docker-compose restart identity

echo.
echo ? Waiting for Keycloak to restart...
timeout /t 30 /nobreak > nul

echo.
echo ?? Checking Keycloak health after restart...
docker exec eshop-identity curl -s http://localhost:8080/health/ready 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ? Keycloak is now healthy!
) else (
    echo ? Keycloak is still starting (may take 2-3 minutes)
)

echo.
echo ?? Development environment setup completed!
echo.
echo ??? Development database details:
echo   • Database: eshopdb_dev
echo   • Host: localhost
echo   • Port: 5433
echo   • Username: eshopuser
echo   • Password: EShop123!
echo.
echo ?? Available schemas:
echo   • catalog - Product catalog
echo   • basket - Shopping cart
echo   • ordering - Order processing
echo   • keycloak_dev - Keycloak identity (development)
echo   • shared - Cross-module data
echo   • messaging - Event/message data
echo.
echo ?? Next steps:
echo   1. Wait for Keycloak to fully start (2-3 minutes)
echo   2. Check services: check-services.bat
echo   3. Access development Keycloak: http://localhost:9090

:: Cleanup temporary file
del dev-db-setup.sql 2>nul

pause