@echo off
setlocal enabledelayedexpansion

echo ?? eShop Docker Cleanup Script - Enhanced Version
echo =================================================

:: Get the directory where this script is located
set "SCRIPT_DIR=%~dp0"
echo ?? Script directory: %SCRIPT_DIR%

:: Change to the script directory to ensure docker-compose.yml is found
cd /d "%SCRIPT_DIR%"
echo ?? Working directory: %CD%

echo.
echo ?? Checking Docker status...
docker version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ? Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)
echo ? Docker is running

echo.
echo ?? Checking for docker-compose.yml...
if exist "docker-compose.yml" (
    echo ? Found docker-compose.yml
) else (
    echo ? docker-compose.yml not found in current directory
    echo Current directory: %CD%
    echo Please ensure this script is in the same directory as docker-compose.yml
    pause
    exit /b 1
)

echo.
echo ?? Stopping all Docker Compose services...
docker-compose down --remove-orphans 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ?? No running compose services found or compose file issue
) else (
    echo ? Stopped compose services
)

echo.
echo ??? Removing all eShop containers (running and stopped)...
echo Removing individual containers:

:: Define container list
set "containers=eshop-seq eshop-database eshop-redis eshop-messagebus eshop-identity eshop-pgadmin eshop-application"

for %%c in (%containers%) do (
    docker container rm -f %%c >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo   ? Removed %%c
    ) else (
        echo   ?? %%c not found ^(already removed^)
    )
)

echo.
echo ??? Removing any containers with 'eshop' in the name...
for /f "tokens=*" %%i in ('docker ps -aq --filter "name=eshop" 2^>nul') do (
    docker container rm -f %%i >nul 2>&1
    echo   ? Removed container: %%i
)

echo.
echo ?? Cleaning up orphaned Docker resources...
echo Pruning containers...
docker container prune -f >nul 2>&1
echo Pruning networks...
docker network prune -f >nul 2>&1
echo ? Cleanup completed

echo.
echo ?? Current container status:
set "current_containers="
for /f "tokens=*" %%i in ('docker ps -a --filter "name=eshop" --format "{{.Names}}" 2^>nul') do (
    set "current_containers=found"
    echo Container found: %%i
)
if not defined current_containers (
    echo ? No eShop containers currently exist
)

echo.
echo ?? Validating Docker Compose configuration...
docker-compose config --quiet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ? Docker Compose configuration has errors
    echo Running validation with output:
    docker-compose config
    pause
    exit /b 1
) else (
    echo ? Docker Compose configuration is valid
)

echo.
echo ?? Starting fresh eShop services...
docker-compose up -d

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to start services. Please check the error output above.
    echo.
    echo ?? Attempting to show more details:
    docker-compose up --no-start
    echo.
    echo ?? Try running 'docker-compose logs' to see detailed error messages
    pause
    exit /b 1
) else (
    echo ? Services started successfully
)

echo.
echo ? Waiting for services to initialize...
timeout /t 15 /nobreak > nul

echo.
echo ?? Final service status:
docker-compose ps

echo.
echo ?? Quick health check...
timeout /t 5 /nobreak > nul
curl -s -I http://localhost:8080/health >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ? API health endpoint is responding
) else (
    echo ? API is still starting up or not accessible yet
)

echo.
echo ?? Cleanup and restart completed successfully!
echo.
echo ?? Available endpoints:
echo   • API Health:          http://localhost:8080/health
echo   • API Swagger:         http://localhost:8080/swagger
echo   • API Main:            http://localhost:8080
echo   • API Info:            http://localhost:8080/
echo   • pgAdmin:             http://localhost:5050 ^(admin@eshop.com / admin123^)
echo   • Keycloak Admin:      http://localhost:8082 ^(admin / admin123^)
echo   • RabbitMQ Management: http://localhost:15672 ^(guest / guest^)
echo   • Seq Logging:         http://localhost:5341
echo.
echo ?? Useful commands:
echo   • View all logs:       docker-compose logs
echo   • View API logs:       docker-compose logs -f eshop-api
echo   • View identity logs:  docker-compose logs -f identity
echo   • Stop all services:   docker-compose down
echo   • Restart a service:   docker-compose restart [service-name]
echo   • Check service status: docker-compose ps
echo.
echo ?? If services are still starting, wait a few more minutes before accessing endpoints

pause