@echo off
setlocal enabledelayedexpansion

:: eShop Docker Compose Management Script for Windows

if "%~1"=="" goto :help
if "%~1"=="help" goto :help
if "%~1"=="up" goto :up
if "%~1"=="down" goto :down
if "%~1"=="restart" goto :restart
if "%~1"=="logs" goto :logs
if "%~1"=="logs-api" goto :logs-api
if "%~1"=="logs-db" goto :logs-db
if "%~1"=="status" goto :status
if "%~1"=="clean" goto :clean
if "%~1"=="build" goto :build
if "%~1"=="dev" goto :dev
goto :help

:help
echo.
echo ===============================================
echo eShop Docker Compose Management
echo ===============================================
echo.
echo Usage: %~nx0 [COMMAND]
echo.
echo Commands:
echo   up          Start all services
echo   down        Stop all services
echo   restart     Restart all services
echo   logs        Show logs for all services
echo   logs-api    Show logs for API only
echo   logs-db     Show logs for database only
echo   status      Show status of all services
echo   clean       Stop services and remove volumes
echo   build       Build and start services
echo   dev         Start development environment (infrastructure only)
echo   help        Show this help message
echo.
goto :end

:up
echo Starting eShop services...
docker-compose up -d
if %ERRORLEVEL% EQU 0 (
    echo Services started successfully!
    call :show_access_info
) else (
    echo Failed to start services!
)
goto :end

:down
echo Stopping eShop services...
docker-compose down
if %ERRORLEVEL% EQU 0 (
    echo Services stopped successfully!
) else (
    echo Failed to stop services!
)
goto :end

:restart
echo Restarting eShop services...
docker-compose restart
if %ERRORLEVEL% EQU 0 (
    echo Services restarted successfully!
) else (
    echo Failed to restart services!
)
goto :end

:logs
echo Showing logs for all services...
docker-compose logs -f
goto :end

:logs-api
echo Showing logs for API service...
docker-compose logs -f eshop-api
goto :end

:logs-db
echo Showing logs for database service...
docker-compose logs -f eshop-db
goto :end

:status
echo Service Status:
docker-compose ps
goto :end

:clean
echo WARNING: This will remove all data!
set /p confirm="Are you sure? (y/N): "
if /i "%confirm%"=="y" (
    docker-compose down -v
    echo Environment cleaned successfully!
) else (
    echo Operation cancelled.
)
goto :end

:build
echo Building and starting eShop services...
docker-compose up -d --build
if %ERRORLEVEL% EQU 0 (
    echo Services built and started successfully!
    call :show_access_info
) else (
    echo Failed to build and start services!
)
goto :end

:dev
echo Starting development environment (infrastructure only)...
docker-compose up -d eshop-db eshop-redis eshop-seq eshop-pgadmin
if %ERRORLEVEL% EQU 0 (
    echo Development infrastructure started!
    echo You can now run the API locally with: dotnet run --project Bootstrapper/Api
    call :show_access_info
) else (
    echo Failed to start development infrastructure!
)
goto :end

:show_access_info
echo.
echo ========================================
echo eShop Services Access Information
echo ========================================
echo.
echo API Application:
echo    HTTP:  http://localhost:8080
echo    HTTPS: https://localhost:8081
echo    Health: http://localhost:8080/health
echo.
echo Database (PostgreSQL):
echo    Host: localhost:5432
echo    Database: eshopdb
echo    Username: eshopuser
echo    Password: eshoppass123
echo.
echo pgAdmin (Database Management):
echo    URL: http://localhost:5050
echo    Email: admin@eshop.com
echo    Password: admin123
echo.
echo Seq (Logging):
echo    URL: http://localhost:5341
echo.
echo Redis Cache:
echo    Host: localhost:6379
echo.
goto :eof

:end
endlocal