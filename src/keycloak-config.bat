@echo off
echo ===============================================
echo ?? Keycloak Database Configuration Helper
echo ===============================================

echo.
echo ?? Available Databases:
docker exec eshop-database psql -U eshopuser -d postgres -c "\l"

echo.
echo ?? Keycloak Database Connection Options:
echo.
echo 1. Using Environment Variables (Current Docker Setup):
echo    KC_DB=postgres
echo    KC_DB_URL_HOST=eshop-db
echo    KC_DB_URL_PORT=5432
echo    KC_DB_URL_DATABASE=identitydb_dev
echo    KC_DB_USERNAME=eshopuser
echo    KC_DB_PASSWORD=EShop123!
echo.
echo 2. Using JDBC URL (Alternative):
echo    --db postgres --db-url jdbc:postgresql://eshop-db:5432/identitydb_dev
echo.
echo 3. For External Connection (from host):
echo    --db postgres --db-url jdbc:postgresql://localhost:5433/identitydb_dev
echo.

echo ===============================================
echo ?? Testing Database Connections:
echo ===============================================

echo.
echo Testing Business Database (eshopdb_dev):
docker exec eshop-database psql -U eshopuser -d eshopdb_dev -c "SELECT current_database(), current_user, version();" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ? Business Database Connection: SUCCESS
) else (
    echo ? Business Database Connection: FAILED
)

echo.
echo Testing Identity Database (identitydb_dev):
docker exec eshop-database psql -U eshopuser -d identitydb_dev -c "SELECT current_database(), current_user, version();" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ? Identity Database Connection: SUCCESS
) else (
    echo ? Identity Database Connection: FAILED
    echo Creating identitydb_dev database...
    docker exec eshop-database psql -U eshopuser -d postgres -c "CREATE DATABASE identitydb_dev OWNER eshopuser;"
)

echo.
echo Testing Production Identity Database (identitydb):
docker exec eshop-database psql -U eshopuser -d identitydb -c "SELECT current_database(), current_user, version();" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ? Production Identity Database Connection: SUCCESS
) else (
    echo ? Production Identity Database Connection: FAILED
)

echo.
echo ===============================================
echo ?? Current Keycloak Container Status:
echo ===============================================
docker ps --filter "name=eshop-identity" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo ===============================================
echo ?? Keycloak Connection String Templates:
echo ===============================================
echo.
echo Development Environment:
echo   JDBC URL: jdbc:postgresql://eshop-db:5432/identitydb_dev
echo   External: jdbc:postgresql://localhost:5433/identitydb_dev
echo.
echo Production Environment:
echo   JDBC URL: jdbc:postgresql://eshop-db:5432/identitydb
echo   External: jdbc:postgresql://localhost:5433/identitydb
echo.

pause