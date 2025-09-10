@echo off
echo ===============================================
echo ???  Identity Database Setup
echo ===============================================

echo.
echo ?? Checking current databases...
docker exec eshop-database psql -U eshopuser -d postgres -c "\l"

echo.
echo ???  Creating identity databases if they don't exist...

echo Creating development identity database...
docker exec eshop-database psql -U eshopuser -d postgres -c "SELECT 1 FROM pg_database WHERE datname = 'identitydb_dev'" | findstr "1" >nul
if %ERRORLEVEL% NEQ 0 (
    echo Creating identitydb_dev...
    docker exec eshop-database psql -U eshopuser -d postgres -c "CREATE DATABASE identitydb_dev OWNER eshopuser;"
    echo ? identitydb_dev created
) else (
    echo ??  identitydb_dev already exists
)

echo.
echo Creating production identity database...
docker exec eshop-database psql -U eshopuser -d postgres -c "SELECT 1 FROM pg_database WHERE datname = 'identitydb'" | findstr "1" >nul
if %ERRORLEVEL% NEQ 0 (
    echo Creating identitydb...
    docker exec eshop-database psql -U eshopuser -d postgres -c "CREATE DATABASE identitydb OWNER eshopuser;"
    echo ? identitydb created
) else (
    echo ??  identitydb already exists
)

echo.
echo ?? Setting up database permissions...
docker exec eshop-database psql -U eshopuser -d identitydb_dev -c "GRANT ALL PRIVILEGES ON DATABASE identitydb_dev TO eshopuser;"
docker exec eshop-database psql -U eshopuser -d identitydb -c "GRANT ALL PRIVILEGES ON DATABASE identitydb TO eshopuser;"

echo.
echo ?? Final database list:
docker exec eshop-database psql -U eshopuser -d postgres -c "\l"

echo.
echo ===============================================
echo ?? Identity Database Setup Complete!
echo ===============================================
echo.
echo You now have:
echo ? eshopdb_dev    - Business data (catalog, basket, ordering, etc.)
echo ? identitydb_dev - Development identity/authentication data  
echo ? identitydb     - Production identity/authentication data
echo.

pause