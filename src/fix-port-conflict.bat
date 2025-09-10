@echo off
echo ?? Port Conflict Resolver - PostgreSQL Port 5432
echo ================================================

echo.
echo ?? ISSUE: Port 5432 is already in use
echo This usually happens when:
echo   1. Another PostgreSQL instance is running
echo   2. Previous Docker containers weren't properly cleaned up
echo   3. Other applications are using port 5432

echo.
echo ?? Step 1: Checking what's using port 5432...
netstat -ano | findstr :5432
if %ERRORLEVEL% EQU 0 (
    echo ? Found processes using port 5432 (shown above)
) else (
    echo ?? No processes found using port 5432 (might be Docker internal)
)

echo.
echo ?? Step 2: Checking for PostgreSQL services...
sc query postgresql* 2>nul | findstr "SERVICE_NAME\|STATE"
if %ERRORLEVEL% EQU 0 (
    echo ?? Found PostgreSQL Windows services (shown above)
) else (
    echo ? No PostgreSQL Windows services found
)

echo.
echo ?? Step 3: Checking Docker containers using port 5432...
docker ps -a --filter "publish=5432" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
if %ERRORLEVEL% EQU 0 (
    echo ? Docker container check completed
) else (
    echo ?? No Docker containers found with port 5432
)

echo.
echo ??? RESOLUTION OPTIONS:
echo.
echo Option 1: Kill processes using port 5432
set /p kill_choice="Do you want to kill processes using port 5432? (Y/N): "
if /i "%kill_choice%"=="Y" (
    echo Killing processes on port 5432...
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5432') do (
        echo Killing process %%a
        taskkill /PID %%a /F 2>nul
    )
    echo ? Processes killed
)

echo.
echo Option 2: Stop PostgreSQL Windows services
set /p service_choice="Do you want to stop PostgreSQL Windows services? (Y/N): "
if /i "%service_choice%"=="Y" (
    echo Stopping PostgreSQL services...
    net stop postgresql* 2>nul
    echo ? PostgreSQL services stopped
)

echo.
echo Option 3: Remove conflicting Docker containers
set /p docker_choice="Do you want to remove Docker containers using port 5432? (Y/N): "
if /i "%docker_choice%"=="Y" (
    echo Removing Docker containers...
    for /f "tokens=1" %%a in ('docker ps -aq --filter "publish=5432"') do (
        echo Removing container %%a
        docker container rm -f %%a 2>nul
    )
    echo ? Docker containers removed
)

echo.
echo ?? Step 4: Complete Docker cleanup...
echo Removing all eShop containers...
docker container rm -f eshop-database eshop-redis eshop-messagebus eshop-identity eshop-pgadmin eshop-application eshop-seq 2>nul
docker container prune -f >nul 2>&1
docker network prune -f >nul 2>&1

echo.
echo ?? Step 5: Verifying port 5432 is now free...
netstat -ano | findstr :5432 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ?? Port 5432 is still in use. You may need to:
    echo   1. Restart your computer
    echo   2. Check for hidden PostgreSQL instances
    echo   3. Use a different port in docker-compose.yml
) else (
    echo ? Port 5432 is now free!
)

echo.
echo ?? Step 6: Ready to start services
echo Now you can run: start-services.bat

pause