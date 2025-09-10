@echo off
echo ===============================================
echo ?? COMPLETE APPLICATION REBUILD & RESTART
echo ===============================================

echo.
echo ?? Step 1: Stopping all services...
docker-compose -f docker-compose.yml -f docker-compose.override.yml down

echo.
echo ??? Step 2: Removing application container to force rebuild...
docker rmi src_eshop-api 2>nul
docker rmi src-eshop-api 2>nul

echo.
echo ??? Step 3: Building application with latest code...
docker-compose -f docker-compose.yml -f docker-compose.override.yml build eshop-api --no-cache

echo.
echo ?? Step 4: Starting all services...
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

echo.
echo ?? Step 5: Checking service status...
timeout /t 30 /nobreak >nul
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo ?? Step 6: Checking application logs...
echo Last 20 lines from application:
docker logs eshop-application --tail=20

echo.
echo ===============================================
echo ?? SERVICES STATUS:
echo ===============================================
echo.
echo ?? Keycloak: http://localhost:9090 (admin/admin123)
echo ?? pgAdmin: http://localhost:5050 (dev@eshop.com/devpassword123)
echo ?? Seq Logs: http://localhost:5341
echo ?? RabbitMQ: http://localhost:15672 (guest/guest)
echo ?? eShop API: http://localhost:8080 (if healthy)
echo ?? Health Check: http://localhost:8080/health
echo.

pause