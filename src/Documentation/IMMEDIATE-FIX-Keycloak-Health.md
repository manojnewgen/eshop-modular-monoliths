# ?? IMMEDIATE SOLUTION: Keycloak Dependency Health Issue

## ?? **Current Status Analysis**
Based on the diagnostic I just ran, here's what I found:

1. ? **Keycloak container was running** but in "health: starting" status
2. ? **Root cause identified**: `ERROR: schema "keycloak_dev" does not exist`
3. ?? **Containers were removed** during troubleshooting

## ?? **Immediate Fix - Step by Step**

### **Step 1: Start Services with Development Configuration**
```cmd
start-dev-services.bat
```
This will start all services with the development overrides.

### **Step 2: Wait for Database to Be Ready**
```cmd
# Wait 30-60 seconds for PostgreSQL to fully start
# You can check status with:
docker ps --filter "name=eshop-database"
```

### **Step 3: Create Development Database and Schemas**
```cmd
setup-dev-database.bat
```
This creates:
- `eshopdb_dev` database
- `keycloak_dev` schema (the missing piece!)
- All other required schemas

### **Step 4: Restart Keycloak to Pick Up New Schema**
```cmd
docker-compose restart identity
```

### **Step 5: Verify Everything is Working**
```cmd
check-services.bat
# or
diagnose-keycloak.bat
```

## ? **Quick Manual Fix (If Scripts Don't Work)**

If the automated scripts have issues, here's the manual approach:

### **1. Start Services**
```cmd
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### **2. Create Missing Database**
```cmd
# Wait for PostgreSQL to start, then:
docker exec -i src_eshop-db_1 psql -U postgres -c "CREATE DATABASE eshopdb_dev;"
docker exec -i src_eshop-db_1 psql -U eshopuser -d eshopdb_dev -c "CREATE SCHEMA keycloak_dev;"
docker exec -i src_eshop-db_1 psql -U eshopuser -d eshopdb_dev -c "GRANT ALL PRIVILEGES ON SCHEMA keycloak_dev TO eshopuser;"
```

### **3. Restart Keycloak**
```cmd
docker-compose restart identity
```

## ?? **Why This Happened**

The development override file (`docker-compose.override.yml`) configures Keycloak to use:
- Database: `eshopdb_dev` 
- Schema: `keycloak_dev`

But these didn't exist yet, causing Keycloak to fail its health check.

## ? **Expected Timeline**

After running the fix:
1. **PostgreSQL**: Ready in ~30 seconds
2. **Database/Schema Creation**: ~10 seconds  
3. **Keycloak Restart**: ~2-3 minutes to become healthy
4. **Other Services**: ~1-2 minutes total

## ?? **Final Verification**

Once everything is working, you should be able to access:

| Service | URL | Status Check |
|---------|-----|--------------|
| **API** | http://localhost:8080 | http://localhost:8080/health |
| **Keycloak** | http://localhost:9090 | Should show Keycloak login |
| **pgAdmin** | http://localhost:5050 | Connect to eshopdb_dev |
| **Database** | localhost:5433 | eshopdb_dev should exist |

## ?? **Prevention for Future**

To avoid this issue in the future:
1. Always run `setup-dev-database.bat` after starting development services
2. Use `start-dev-services.bat` which includes guidance about this
3. The development database setup only needs to be done once

## ?? **If It Still Fails**

If Keycloak is still unhealthy after these steps:

1. **Check logs**: `docker logs eshop-identity`
2. **Verify schema exists**: 
   ```cmd
   docker exec eshop-database psql -U eshopuser -d eshopdb_dev -c "\dn"
   ```
3. **Try a clean restart**:
   ```cmd
   docker-compose down
   cleanup-docker.bat
   start-dev-services.bat
   setup-dev-database.bat
   ```

## ?? **Expected Result**

After following these steps:
- ? Keycloak will be healthy
- ? All dependent services will start
- ? Your .NET 8 eShop API will be running
- ? Development environment fully operational

This resolves the `dependency failed to start: container eshop-identity is unhealthy` error completely! ??