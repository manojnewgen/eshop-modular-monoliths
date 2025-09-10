@echo off
echo ===============================================
echo ?? PORT CONFLICT FIX - Complete Clean Restart
echo ===============================================

echo.
echo ?? Step 1: Stopping all containers...
docker-compose -f docker-compose.yml -f docker-compose.override.yml down

echo.
echo ?? Step 2: Removing any orphaned containers...
docker ps -a --filter "name=eshop-" --format "{{.Names}}" 2>nul | findstr "eshop-" >nul
if %ERRORLEVEL% EQU 0 (
    echo Found orphaned containers, removing them...
    for /f %%i in ('docker ps -a --filter "name=eshop-" --format "{{.Names}}"') do docker rm -f %%i
) else (
    echo No orphaned containers found.
)

echo.
echo ?? Step 3: Checking port availability...
netstat -ano | findstr :8080 >nul
if %ERRORLEVEL% EQU 0 (
    echo ?? WARNING: Port 8080 is still in use
    echo Ports in use:
    netstat -ano | findstr :8080
) else (
    echo ? Port 8080 is now available
)

echo.
echo ?? Step 4: Starting all services fresh...
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

echo.
echo ?? Step 5: Monitoring startup...
timeout /t 30 /nobreak >nul
echo Current container status:
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo ?? Step 6: Testing connectivity...
echo Testing application health...
timeout /t 10 /nobreak >nul
curl -s -I http://localhost:8080/health 2>nul | findstr "HTTP" >nul
if %ERRORLEVEL% EQU 0 (
    echo ? Application is responding at http://localhost:8080
) else (
    echo ?? Application health check failed, checking logs...
    echo Last 10 lines from application:
    docker logs eshop-application --tail=10
)

echo.
echo ===============================================
echo ?? ACCESS POINTS:
echo ===============================================
echo ?? eShop API: http://localhost:8080
echo ?? Keycloak: http://localhost:9090 (admin/admin123)
echo ?? pgAdmin: http://localhost:5050 (dev@eshop.com/devpassword123)
echo ?? Seq Logs: http://localhost:5341
echo ?? RabbitMQ: http://localhost:15672 (guest/guest)
echo ?? Health Check: http://localhost:8080/health
echo.

pause