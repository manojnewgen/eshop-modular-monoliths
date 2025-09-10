@echo off
setlocal enabledelayedexpansion

echo ?? eShop Docker Services Starter - Enhanced
echo ==========================================

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
echo ?? Looking for docker-compose.yml...
if exist "docker-compose.yml" (
    echo ? Found docker-compose.yml in current directory
) else (
    echo ? docker-compose.yml not found in current directory!
    echo.
    echo ?? Please navigate to the directory containing docker-compose.yml
    echo    Usually this is in the root of your project (where you cloned the repo)
    echo.
    echo ?? Searching for docker-compose.yml in parent directories...
    
    if exist "..\docker-compose.yml" (
        echo ? Found docker-compose.yml in parent directory
        cd ..
        echo ?? Changed to: %CD%
    ) else if exist "..\..\docker-compose.yml" (
        echo ? Found docker-compose.yml in grandparent directory  
        cd ..\..
        echo ?? Changed to: %CD%
    ) else (
        echo ? Could not locate docker-compose.yml
        echo.
        echo ?? Please manually navigate to the correct directory and run:
        echo    docker-compose up -d
        pause
        exit /b 1
    )
)

echo.
echo ?? Validating Docker Compose configuration...
docker-compose config --quiet 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ? Docker Compose configuration has errors
    echo.
    echo ?? Showing configuration details:
    docker-compose config
    pause
    exit /b 1
) else (
    echo ? Configuration is valid
)

echo.
echo ?? Starting eShop services...
docker-compose up -d

if %ERRORLEVEL% NEQ 0 (
    echo ? Failed to start services
    echo.
    echo ?? Common issues and solutions:
    echo   1. File mounting issues - This is often due to Docker Desktop file sharing
    echo   2. Port conflicts - Check if ports 8080, 5432, 6379 are already in use
    echo   3. Insufficient resources - Ensure Docker has enough memory allocated
    echo.
    echo ?? Showing detailed error information:
    docker-compose up --no-start
    pause
    exit /b 1
)

echo.
echo ? Waiting for services to initialize...
timeout /t 15 /nobreak > nul

echo.
echo ?? Service Status:
docker-compose ps

echo.
echo ?? Quick health checks...
timeout /t 5 /nobreak > nul

:: Check PostgreSQL
docker exec eshop-database pg_isready -U eshopuser -d eshopdb >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ? PostgreSQL is ready
) else (
    echo ? PostgreSQL is still starting up
)

:: Check API (might take longer to start)
curl -s -I http://localhost:8080/health >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ? API health endpoint is responding
) else (
    echo ? API is still starting up (this is normal, may take 2-3 minutes)
)

echo.
echo ?? Services startup completed!
echo.
echo ??  IMPORTANT: Database Schema Setup Required
echo.
echo The database started successfully, but the schema setup files couldn't be mounted
echo due to Docker file system restrictions. Please run the database setup manually:
echo.
echo ??? To set up the database schemas, run:
echo    setup-database.bat
echo.
echo This will create all required schemas and sample data.

echo.
echo ?? Available endpoints (may take a few minutes to be fully ready):
echo   • API Health:     http://localhost:8080/health
echo   • API Swagger:    http://localhost:8080/swagger  
echo   • API Main:       http://localhost:8080
echo   • pgAdmin:        http://localhost:5050 (admin@eshop.com / admin123)
echo   • Keycloak:       http://localhost:8082 (admin / admin123)
echo   • RabbitMQ:       http://localhost:15672 (guest / guest)
echo   • Seq Logging:    http://localhost:5341
echo.
echo ?? Next steps:
echo   1. Wait 2-3 minutes for all services to fully start
echo   2. Run: setup-database.bat
echo   3. Test API: http://localhost:8080/health
echo   4. Access Swagger: http://localhost:8080/swagger

pause