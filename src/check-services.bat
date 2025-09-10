@echo off
echo ?? eShop Services Health Check
echo ===============================

echo.
echo ?? Checking Docker service status...
docker-compose ps

echo.
echo ?? Testing service endpoints...

echo.
echo ??? PostgreSQL Database (Port 5433):
docker exec eshop-database pg_isready -U eshopuser -d eshopdb 2>nul
if %ERRORLEVEL% EQU 0 (
    echo   ? PostgreSQL is healthy and accepting connections
    echo   ?? Connection: localhost:5433 (updated port to avoid conflicts)
) else (
    echo   ? PostgreSQL is not ready
)

echo.
echo ?? API Health Check:
curl -s http://localhost:8080/health >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ? API health endpoint is responding
    echo   ?? http://localhost:8080/health
) else (
    echo   ? API is not yet responding (may still be starting)
)

echo.
echo ?? Redis Cache:
docker exec eshop-redis redis-cli ping >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ? Redis is responding
) else (
    echo   ? Redis is not responding
)

echo.
echo ?? RabbitMQ:
curl -s http://localhost:15672 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ? RabbitMQ Management UI is accessible
    echo   ?? http://localhost:15672 (guest/guest)
) else (
    echo   ? RabbitMQ Management UI is not accessible
)

echo.
echo ?? Keycloak Identity Server:
curl -s http://localhost:8082 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ? Keycloak is accessible
    echo   ?? http://localhost:8082 (admin/admin123)
) else (
    echo   ? Keycloak is not yet accessible (may still be starting)
)

echo.
echo ??? pgAdmin:
curl -s http://localhost:5050 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ? pgAdmin is accessible
    echo   ?? http://localhost:5050 (admin@eshop.com/admin123)
    echo   ?? Database connection: localhost:5433
) else (
    echo   ? pgAdmin is not accessible
)

echo.
echo ?? Seq Logging:
curl -s http://localhost:5341 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo   ? Seq logging interface is accessible
    echo   ?? http://localhost:5341
) else (
    echo   ? Seq is not accessible
)

echo.
echo ??? Database Schema Check:
echo Checking if database schemas exist...
docker exec eshop-database psql -U eshopuser -d eshopdb -c "\dn" 2>nul | findstr -i "catalog\|basket\|ordering" >nul
if %ERRORLEVEL% EQU 0 (
    echo   ? Database schemas are present
) else (
    echo   ?? Database schemas not found - run setup-database.bat
)

echo.
echo ===============================
echo ?? Summary:
echo.
echo ?? Access your services:
echo   • API:        http://localhost:8080
echo   • Health:     http://localhost:8080/health
echo   • Swagger:    http://localhost:8080/swagger
echo   • pgAdmin:    http://localhost:5050
echo   • Keycloak:   http://localhost:8082
echo   • RabbitMQ:   http://localhost:15672
echo   • Seq Logs:   http://localhost:5341
echo.
echo ??? Database connection:
echo   • Host: localhost
echo   • Port: 5433 (updated to avoid conflicts with system PostgreSQL)
echo   • Database: eshopdb
echo   • Username: eshopuser
echo   • Password: EShop123!
echo.
echo ?? If any service shows as not ready, wait a few minutes and run this script again.

pause