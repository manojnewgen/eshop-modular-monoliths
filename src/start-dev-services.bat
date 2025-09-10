@echo off
setlocal enabledelayedexpansion

echo ?? eShop Development Environment Starter
echo ========================================

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
echo ?? Looking for docker-compose files...
if exist "docker-compose.yml" (
    echo ? Found docker-compose.yml
) else (
    echo ? docker-compose.yml not found in current directory!
    pause
    exit /b 1
)

if exist "docker-compose.override.yml" (
    echo ? Found docker-compose.override.yml (development overrides)
    set USE_OVERRIDE=true
) else (
    echo ?? No docker-compose.override.yml found, using base configuration only
    set USE_OVERRIDE=false
)

echo.
echo ?? Cleaning up any existing containers...
docker-compose down --remove-orphans 2>nul

echo.
echo ??? Removing conflicting containers...
docker container rm -f eshop-database eshop-redis eshop-messagebus eshop-identity eshop-pgadmin eshop-application eshop-seq 2>nul
docker container prune -f >nul 2>&1

echo.
echo ?? Validating Docker Compose configuration...
if "%USE_OVERRIDE%"=="true" (
    docker-compose -f docker-compose.yml -f docker-compose.override.yml config --quiet 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo ? Docker Compose configuration has errors with override file
        echo Trying base configuration only...
        docker-compose config --quiet 2>nul
        if %ERRORLEVEL% NEQ 0 (
            echo ? Base Docker Compose configuration also has errors
            pause
            exit /b 1
        ) else (
            echo ?? Using base configuration only due to override file issues
            set USE_OVERRIDE=false
        )
    ) else (
        echo ? Configuration is valid with development overrides
    )
) else (
    docker-compose config --quiet 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo ? Docker Compose configuration has errors
        pause
        exit /b 1
    ) else (
        echo ? Base configuration is valid
    )
)

echo.
echo ?? Starting eShop services...
if "%USE_OVERRIDE%"=="true" (
    echo Using development configuration with overrides...
    docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
) else (
    echo Using base configuration...
    docker-compose up -d
)

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to start services
    echo.
    echo ?? Common issues and solutions:
    echo   1. Port conflicts - Run fix-port-conflict.bat
    echo   2. File mounting issues - Check file permissions and Docker file sharing
    echo   3. Keycloak startup issues - Run diagnose-keycloak.bat
    echo.
    pause
    exit /b 1
)

echo.
echo ? Waiting for services to initialize...
echo Note: Keycloak may take 2-3 minutes to start in development mode
timeout /t 20 /nobreak > nul

echo.
echo ?? Service Status:
docker-compose ps

echo.
echo ?? Quick health checks...
timeout /t 5 /nobreak > nul

:: Check PostgreSQL
docker exec eshop-database pg_isready -U eshopuser -d eshopdb_dev >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ? PostgreSQL is ready (Development DB: eshopdb_dev)
) else (
    echo ? PostgreSQL is still starting up
)

:: Check Keycloak (development mode)
curl -s -I http://localhost:9090 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ? Keycloak is responding on development port 9090
) else (
    echo ? Keycloak is still starting up (this can take 2-3 minutes)
)

:: Check API
curl -s -I http://localhost:8080/health >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ? API health endpoint is responding
) else (
    echo ? API is still starting up
)

echo.
echo ?? Services startup initiated!
echo.
echo ?? IMPORTANT NOTES:
echo.
echo ??? Database Setup Required:
echo   Run: setup-database.bat (use eshopdb_dev for development)
echo.
echo ? Startup Times:
echo   • PostgreSQL: ~30 seconds
echo   • Redis/RabbitMQ: ~1 minute  
echo   • Keycloak: ~2-3 minutes (development mode)
echo   • API: ~1-2 minutes (after all dependencies are ready)

echo.
echo ?? Development endpoints:
if "%USE_OVERRIDE%"=="true" (
    echo   • API:        http://localhost:8080
    echo   • Swagger:    http://localhost:8080/swagger
    echo   • Keycloak:   http://localhost:9090 ^(admin/admin123^)
    echo   • pgAdmin:    http://localhost:5050 ^(dev@eshop.com/devpassword123^)
    echo   • RabbitMQ:   http://localhost:15672 ^(guest/guest^)
    echo   • Database:   localhost:5433 ^(eshopdb_dev^)
) else (
    echo   • API:        http://localhost:8080
    echo   • Swagger:    http://localhost:8080/swagger
    echo   • Keycloak:   http://localhost:8082 ^(admin/admin123^)
    echo   • pgAdmin:    http://localhost:5050 ^(admin@eshop.com/admin123^)
    echo   • RabbitMQ:   http://localhost:15672 ^(guest/guest^)
    echo   • Database:   localhost:5433 ^(eshopdb^)
)

echo.
echo ?? Troubleshooting tools:
echo   • Check all services:     check-services.bat
echo   • Diagnose Keycloak:      diagnose-keycloak.bat
echo   • Fix port conflicts:     fix-port-conflict.bat
echo   • Setup database:         setup-database.bat

pause