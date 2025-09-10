@echo off
echo ===============================================
echo ?? PGADMIN - KEYCLOAK TABLES LOCATION GUIDE
echo ===============================================

echo.
echo ?? LOCATING KEYCLOAK TABLES:
echo ===============================================

echo.
echo ?? Checking identitydb_dev for Keycloak tables:
docker exec eshop-database psql -U eshopuser -d identitydb_dev -c "SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename LIKE '%%user%%' OR tablename LIKE '%%realm%%' OR tablename LIKE '%%client%%' LIMIT 10;"

echo.
echo ?? Total Keycloak tables in identitydb_dev:
docker exec eshop-database psql -U eshopuser -d identitydb_dev -c "SELECT count(*) as keycloak_tables FROM pg_tables WHERE schemaname = 'public';"

echo.
echo ===============================================
echo ?? PGADMIN CONNECTION INSTRUCTIONS:
echo ===============================================
echo.
echo 1. Open pgAdmin: http://localhost:5050
echo    Login: dev@eshop.com
echo    Password: devpassword123
echo.
echo 2. Add New Server Connection:
echo    Name: "Keycloak Identity Database"
echo    
echo    Connection Tab:
echo    ???????????????????????????????????????
echo    ? Host: eshop-database                ?
echo    ? Port: 5432                          ?
echo    ? Maintenance DB: identitydb_dev      ?
echo    ? Username: eshopuser                 ?
echo    ? Password: EShop123!                 ?
echo    ???????????????????????????????????????
echo.
echo 3. Alternative (External Connection):
echo    ???????????????????????????????????????
echo    ? Host: localhost                     ?
echo    ? Port: 5433                          ?
echo    ? Maintenance DB: identitydb_dev      ?
echo    ? Username: eshopuser                 ?
echo    ? Password: EShop123!                 ?
echo    ???????????????????????????????????????
echo.
echo 4. Navigate to: Servers ? Keycloak Identity Database ? Databases ? identitydb_dev ? Schemas ? public ? Tables
echo.
echo ===============================================
echo ?? EXPECTED KEYCLOAK TABLES:
echo ===============================================
echo.
echo You should see tables like:
echo • user_entity (users)
echo • realm (realms/tenants)
echo • client (OAuth clients)
echo • keycloak_role (roles)
echo • user_role_mapping (user-role assignments)
echo • admin_event_entity (admin events)
echo • authentication_flow (auth flows)
echo • And 85+ more Keycloak tables...
echo.
echo ===============================================
echo ??? DATABASE VERIFICATION:
echo ===============================================

echo.
echo Current Database Status:
docker exec eshop-database psql -U eshopuser -d postgres -c "SELECT datname, pg_size_pretty(pg_database_size(datname)) as size FROM pg_database WHERE datname IN ('eshopdb_dev', 'identitydb_dev', 'identitydb') ORDER BY datname;"

echo.
echo Keycloak Table Sample:
docker exec eshop-database psql -U eshopuser -d identitydb_dev -c "SELECT 'user_entity' as table_name, count(*) as records FROM user_entity UNION SELECT 'realm' as table_name, count(*) as records FROM realm UNION SELECT 'client' as table_name, count(*) as records FROM client;"

echo.
echo ===============================================
echo ?? TROUBLESHOOTING:
echo ===============================================
echo.
echo If you don't see Keycloak tables:
echo 1. Refresh pgAdmin connection (right-click server ? Refresh)
echo 2. Make sure you're connected to identitydb_dev (not eshopdb_dev)
echo 3. Expand: Databases ? identitydb_dev ? Schemas ? public ? Tables
echo 4. If still empty, run: docker-compose restart identity
echo.
echo ===============================================

pause