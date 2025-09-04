# ?? PostgreSQL Container Health Check Fix

## Issue: "dependency failed to start: container eshop-database is unhealthy"

The PostgreSQL container is failing its health check, which prevents other services from starting.

## ?? **Immediate Diagnosis**

Run these commands to diagnose the exact issue:

```powershell
# Check container status
docker-compose ps

# Check PostgreSQL container logs
docker-compose logs eshop-db

# Check if container is running
docker inspect eshop-database

# Try manual health check
docker-compose exec eshop-db pg_isready -U eshopuser -d eshopdb
```

## ?? **Quick Fix Steps**

### **Step 1: Stop and Clean**
```powershell
# Stop all services
docker-compose down

# Remove volumes (will delete data but fix permissions)
docker volume rm eshop_postgres_data

# Clean system
docker system prune -f
```

### **Step 2: Fix Docker Compose Configuration**
The issue is likely with the health check timing or volume permissions. Update your docker-compose.yml:

```yaml
# Fixed PostgreSQL service
eshop-db:
  image: postgres:15.3
  platform: linux/amd64
  container_name: eshop-database
  restart: unless-stopped
  environment:
    POSTGRES_DB: eshopdb
    POSTGRES_USER: eshopuser
    POSTGRES_PASSWORD: eshoppass123
    POSTGRES_INITDB_ARGS: "--auth-host=scram-sha-256 --auth-local=scram-sha-256"
  ports:
    - "5432:5432"
  volumes:
    - postgres_eshopdb:/var/lib/postgresql/data
    - ./init-scripts:/docker-entrypoint-initdb.d
  networks:
    - eshop-network
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U eshopuser -d eshopdb -h localhost"]
    interval: 10s
    timeout: 5s
    retries: 5
    start_period: 60s  # Increased from 30s
```

### **Step 3: Start Services Incrementally**
```powershell
# Start database only first
docker-compose up -d eshop-db

# Wait and check logs
docker-compose logs -f eshop-db

# Once healthy, start other services
docker-compose up -d
```

## ??? **Advanced Troubleshooting**

### **Check Database Logs for Specific Errors:**
```powershell
# Check for permission errors
docker-compose logs eshop-db | Select-String "permission"

# Check for initialization errors
docker-compose logs eshop-db | Select-String "ERROR"

# Check for authentication errors
docker-compose logs eshop-db | Select-String "authentication"
```

### **Manual Database Connection Test:**
```powershell
# Test connection from host
docker-compose exec eshop-db psql -U eshopuser -d eshopdb -c "SELECT version();"

# Test from another container
docker run --rm --network eshop-network postgres:15.3 pg_isready -h eshop-db -U eshopuser -d eshopdb
```

### **Volume Permission Fix (if needed):**
```powershell
# If you see permission errors, fix with:
docker-compose down
docker volume rm eshop_postgres_data
docker volume create eshop_postgres_data

# Then restart
docker-compose up -d eshop-db
```

## ?? **Alternative Solutions**

### **Solution 1: Simpler PostgreSQL Configuration**
If the above doesn't work, try a simpler configuration:

```yaml
eshop-db:
  image: postgres:15-alpine
  platform: linux/amd64
  container_name: eshop-database
  environment:
    POSTGRES_DB: eshopdb
    POSTGRES_USER: eshopuser
    POSTGRES_PASSWORD: eshoppass123
  ports:
    - "5432:5432"
  volumes:
    - postgres_eshopdb:/var/lib/postgresql/data
  networks:
    - eshop-network
  healthcheck:
    test: ["CMD-SHELL", "pg_isready"]
    interval: 30s
    timeout: 10s
    retries: 3
    start_period: 40s
```

### **Solution 2: Disable Health Check Temporarily**
```yaml
eshop-db:
  # ... other configuration ...
  # Comment out healthcheck temporarily
  # healthcheck:
  #   test: ["CMD-SHELL", "pg_isready -U eshopuser -d eshopdb"]
  #   interval: 30s
  #   timeout: 10s
  #   retries: 3
  #   start_period: 30s
```

### **Solution 3: Use depends_on without health check**
```yaml
eshop-api:
  # Change from:
  depends_on:
    eshop-db:
      condition: service_healthy
  # To:
  depends_on:
    - eshop-db
```

## ?? **Common Causes and Solutions**

| Issue | Symptoms | Solution |
|-------|----------|----------|
| **Slow startup** | Health check timeout | Increase `start_period` to 60s+ |
| **Volume permissions** | Permission denied errors | Remove and recreate volume |
| **Init scripts** | Long initialization | Move init scripts or disable temporarily |
| **Memory/CPU** | Container restart loops | Increase Docker Desktop resources |
| **Network issues** | Connection refused | Check network configuration |

## ?? **Recommended Final Configuration**

Use this tested configuration in your docker-compose.yml:

```yaml
eshop-db:
  image: postgres:15.3
  platform: linux/amd64
  container_name: eshop-database
  restart: unless-stopped
  environment:
    POSTGRES_DB: eshopdb
    POSTGRES_USER: eshopuser
    POSTGRES_PASSWORD: eshoppass123
  ports:
    - "5432:5432"
  volumes:
    - postgres_eshopdb:/var/lib/postgresql/data
    - ./init-scripts:/docker-entrypoint-initdb.d:ro
  networks:
    - eshop-network
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U eshopuser -d eshopdb"]
    interval: 30s
    timeout: 10s
    retries: 5
    start_period: 60s
  command: postgres -c 'max_connections=100' -c 'shared_buffers=128MB'
```

## ?? **Prevention Tips**

1. **Always start database first** and verify it's healthy
2. **Use explicit volume names** and manage them properly
3. **Monitor logs** during startup for early error detection
4. **Keep init scripts simple** and fast-executing
5. **Ensure adequate resources** (RAM/CPU) for Docker Desktop

Your PostgreSQL container should now start successfully! ??