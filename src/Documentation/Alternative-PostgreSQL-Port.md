# ?? Alternative PostgreSQL Port Configuration

## ?? **Issue: Port 5432 Conflict**

If you prefer not to kill existing PostgreSQL services, you can change the Docker port mapping to use a different external port.

## ? **Solution: Use Alternative Port**

### **Option A: Change Docker Compose Port (Recommended)**

Edit your `docker-compose.yml` and change the PostgreSQL port mapping:

```yaml
# From:
ports:
  - "${POSTGRES_PORT:-5432}:5432"

# To:
ports:
  - "${POSTGRES_PORT:-5433}:5432"  # Use port 5433 externally
```

### **Option B: Set Environment Variable**

Create/update your `.env` file with:
```bash
POSTGRES_PORT=5433
```

## ?? **Update Application Configuration**

After changing the port, update your connection strings in:

### **1. docker-compose.yml**
No changes needed - internal container communication still uses port 5432.

### **2. appsettings.json (if connecting externally)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=eshopdb;Username=eshopuser;Password=EShop123!;"
  }
}
```

### **3. pgAdmin Connection**
When connecting to the database via pgAdmin:
- **Host**: localhost
- **Port**: 5433 (or whatever port you chose)
- **Database**: eshopdb
- **Username**: eshopuser
- **Password**: EShop123!

## ?? **Service Access After Port Change**

| Service | Original Port | Alternative Port | URL |
|---------|---------------|------------------|-----|
| **PostgreSQL** | 5432 | 5433 | localhost:5433 |
| **API** | 8080 | 8080 | http://localhost:8080 |
| **pgAdmin** | 5050 | 5050 | http://localhost:5050 |
| **Keycloak** | 8082 | 8082 | http://localhost:8082 |

## ?? **Quick Implementation**

1. **Stop any running services**:
   ```cmd
   docker-compose down
   ```

2. **Edit .env file** (create if it doesn't exist):
   ```bash
   POSTGRES_PORT=5433
   ```

3. **Start services**:
   ```cmd
   start-services.bat
   ```

## ?? **Advantages of This Approach**

? **Non-disruptive** - Doesn't affect existing PostgreSQL installations  
? **Safe** - No need to kill system processes  
? **Flexible** - Easy to change back if needed  
? **Clean** - Maintains separation between system and Docker services  

This approach allows you to run your eShop development environment alongside any existing PostgreSQL installations without conflicts.