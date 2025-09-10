@echo off
echo ??? eShop Database Schema Setup
echo ===============================

echo.
echo ?? Checking if PostgreSQL container is running...
docker ps --filter "name=eshop-database" --format "table {{.Names}}\t{{.Status}}" | findstr "eshop-database"
if %ERRORLEVEL% NEQ 0 (
    echo ? PostgreSQL container is not running
    echo Please start your Docker services first using:
    echo   start-services.bat
    pause
    exit /b 1
)
echo ? PostgreSQL container is running

echo.
echo ?? Waiting for PostgreSQL to be ready...
timeout /t 5 /nobreak > nul

echo.
echo ??? Setting up database schemas...
echo This will create the required schemas for the eShop modules

echo.
echo ?? What this script will do:
echo   • Create schemas: catalog, basket, ordering, identity, shared, messaging
echo   • Set up proper permissions for eshopuser
echo   • Install required PostgreSQL extensions
echo   • Create sample tables and data

echo.
echo ?? Note: PostgreSQL is now running on port 5433 (changed to avoid conflicts)

echo.
set /p confirm="Do you want to proceed? (Y/N): "
if /i "%confirm%" NEQ "Y" (
    echo Operation cancelled.
    pause
    exit /b 0
)

echo.
echo ?? Executing database setup...

:: Copy the SQL file into the running container and execute it
docker cp database-schema-setup.sql eshop-database:/tmp/schema-setup.sql

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to copy SQL file to container
    pause
    exit /b 1
)

:: Execute the SQL script inside the container
docker exec -i eshop-database psql -U eshopuser -d eshopdb -f /tmp/schema-setup.sql

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to execute database setup script
    echo.
    echo ?? Troubleshooting tips:
    echo   1. Ensure PostgreSQL is fully started (wait 1-2 minutes)
    echo   2. Check if the database credentials are correct
    echo   3. Verify the container is healthy: docker-compose ps
    pause
    exit /b 1
)

echo.
echo ? Database schema setup completed successfully!

echo.
echo ??? Verifying schema creation...
docker exec -i eshop-database psql -U eshopuser -d eshopdb -c "\dn"

echo.
echo ?? Database statistics:
docker exec -i eshop-database psql -U eshopuser -d eshopdb -c "SELECT schemaname, tablename FROM pg_tables WHERE schemaname IN ('catalog', 'basket', 'ordering', 'shared') ORDER BY schemaname, tablename;"

echo.
echo ?? eShop database is now ready for use!
echo.
echo ?? Database connection details:
echo   • Host: localhost
echo   • Port: 5433 (updated to avoid conflicts)
echo   • Database: eshopdb
echo   • Username: eshopuser
echo   • Password: EShop123!
echo.
echo ?? You can now access:
echo   • pgAdmin: http://localhost:5050 (admin@eshop.com / admin123)
echo   • Connect to: localhost:5433 (not the default 5432)

pause