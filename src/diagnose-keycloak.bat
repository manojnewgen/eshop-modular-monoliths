@echo off
echo ?? Keycloak Identity Server Diagnostic Tool
echo ==========================================

echo.
echo ?? Checking Keycloak container status...
docker ps -a --filter "name=eshop-identity" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo ?? Checking Keycloak health...
docker exec eshop-identity curl -s http://localhost:8080/health/ready 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ? Keycloak health endpoint is responding
) else (
    echo ? Keycloak health endpoint is not responding
)

echo.
echo ?? Recent Keycloak logs (last 50 lines):
echo ----------------------------------------
docker logs --tail=50 eshop-identity

echo.
echo ??? Checking database connectivity from Keycloak...
docker exec eshop-identity timeout 5 bash -c 'cat < /dev/null > /dev/tcp/eshop-db/5432' 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ? Keycloak can connect to database
) else (
    echo ? Keycloak cannot connect to database
)

echo.
echo ?? Checking database for Keycloak schema...
docker exec eshop-database psql -U eshopuser -d eshopdb_dev -c "\dn" 2>nul | findstr keycloak
if %ERRORLEVEL% EQU 0 (
    echo ? Keycloak schema exists in database
) else (
    echo ?? Keycloak schema not found - this is normal for first startup
)

echo.
echo ?? Testing Keycloak external access...
curl -s -I http://localhost:9090 2>nul | findstr "HTTP"
if %ERRORLEVEL% EQU 0 (
    echo ? Keycloak is accessible externally on port 9090
) else (
    echo ? Keycloak is not accessible externally
)

echo.
echo ?? Environment variables check...
docker exec eshop-identity printenv | findstr "KC_\|KEYCLOAK_"

echo.
echo ==========================================
echo ?? Troubleshooting suggestions:
echo.
echo If Keycloak is unhealthy:
echo 1. Check logs above for specific error messages
echo 2. Ensure database is running and accessible
echo 3. Wait longer - Keycloak can take 2-3 minutes to start
echo 4. Try restarting just Keycloak: docker-compose restart identity
echo.
echo If database connection fails:
echo 1. Ensure PostgreSQL container is healthy
echo 2. Check if eshopdb_dev database exists
echo 3. Verify network connectivity between containers
echo.
echo ?? To restart Keycloak only:
echo   docker-compose restart identity
echo.
echo ?? To view live Keycloak logs:
echo   docker-compose logs -f identity

pause