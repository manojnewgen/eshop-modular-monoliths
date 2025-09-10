@echo off
echo ?? Quick Docker Container Conflict Resolver
echo ==========================================

echo.
echo ??? Removing ALL eShop containers...

:: Force remove all containers with 'eshop' in the name
for /f "tokens=*" %%i in ('docker ps -aq --filter "name=eshop" 2^>nul') do (
    echo Removing container: %%i
    docker container rm -f %%i 2>nul
)

:: Force remove containers by specific names (in case filter didn't catch them)
docker container rm -f eshop-seq 2>nul
docker container rm -f eshop-database 2>nul
docker container rm -f eshop-redis 2>nul
docker container rm -f eshop-messagebus 2>nul
docker container rm -f eshop-identity 2>nul
docker container rm -f eshop-pgadmin 2>nul
docker container rm -f eshop-application 2>nul

echo.
echo ?? Cleaning up Docker resources...
docker container prune -f >nul 2>&1
docker network prune -f >nul 2>&1

echo.
echo ? All eShop containers removed!
echo.
echo ?? Now you can run:
echo    docker-compose up -d
echo.
echo ?? Make sure to run docker-compose from the directory containing docker-compose.yml

pause