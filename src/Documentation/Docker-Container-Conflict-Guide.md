# ?? Docker Container Conflict Resolution Guide

## ?? **Current Issue**
```
Error response from daemon: Conflict. The container name "/eshop-identity" is already in use by container "697fb66547f662002a555a643d87937ed598132b931c9161ec0af953f07bbffc". You have to remove (or rename) that container to be able to reuse that name.
```

## ?? **Why This Happens**

Docker container conflicts occur when:
1. **Previous containers weren't properly cleaned up** - Services were stopped but containers remain
2. **Improper shutdown** - Using `Ctrl+C` or `docker stop` instead of `docker-compose down`
3. **Failed startup** - Previous container creation failed but left orphaned containers
4. **Development iterations** - Multiple start/stop cycles without proper cleanup

## ? **Quick Resolution**

### **Option 1: Use Enhanced Cleanup Script (Recommended)**
Run one of the updated cleanup scripts:

**Batch Script (Windows):**
```cmd
cleanup-docker.bat
```

**PowerShell Script (More Features):**
```powershell
.\cleanup-docker.ps1
```

### **Option 2: Manual Commands**
```cmd
# Stop all compose services
docker-compose down --remove-orphans

# Remove specific conflicting containers
docker container rm -f eshop-identity eshop-seq eshop-database eshop-redis eshop-messagebus eshop-pgadmin eshop-application

# Clean up orphaned resources
docker container prune -f
docker network prune -f

# Start fresh
docker-compose up -d
```

### **Option 3: Nuclear Option (Complete Reset)**
```cmd
# Stop everything
docker stop $(docker ps -aq)

# Remove all containers
docker container rm $(docker container ls -aq)

# Clean up everything
docker system prune -af --volumes

# Start your services
docker-compose up -d
```

## ??? **Enhanced Cleanup Scripts Features**

### **New Batch Script (`cleanup-docker.bat`):**
- ? Docker status verification
- ? Individual container removal with feedback
- ? Orphaned resource cleanup
- ? Configuration validation
- ? Service status reporting
- ? Comprehensive endpoint list

### **New PowerShell Script (`cleanup-docker.ps1`):**
- ? All batch script features plus:
- ? Detailed error handling and reporting
- ? Health checks for key services
- ? Color-coded output for better readability
- ? Exception handling and recovery
- ? Interactive feedback and status updates

## ?? **Prevention Strategies**

### **1. Proper Shutdown Sequence**
```cmd
# ? Correct way to stop services
docker-compose down

# ? Avoid these methods
docker stop container_name
Ctrl+C during docker-compose up
```

### **2. Regular Cleanup Routine**
```cmd
# Weekly maintenance
docker-compose down
docker system prune -f
docker-compose up -d
```

### **3. Development Workflow Best Practices**
```cmd
# Development restart
docker-compose restart eshop-api

# After code changes
docker-compose up -d --build eshop-api

# Check logs during development  
docker-compose logs -f eshop-api
```

### **4. Environment Management**
```cmd
# Different environments
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## ?? **Troubleshooting Commands**

### **Diagnostic Commands**
```cmd
# List all containers (running and stopped)
docker ps -a

# Filter for eShop containers
docker ps -a --filter "name=eshop-"

# Check specific container details
docker inspect eshop-identity

# View container logs
docker logs eshop-identity

# Check Docker Compose status
docker-compose ps

# Validate compose configuration
docker-compose config --quiet
```

### **Network and Volume Issues**
```cmd
# List networks
docker network ls

# Remove unused networks
docker network prune

# List volumes
docker volume ls

# Remove unused volumes
docker volume prune
```

## ?? **Service Dependencies**

Understanding the startup order helps prevent conflicts:

```
1. eshop-db (PostgreSQL) - Foundation database
2. eshop-redis (Cache) - Caching layer
3. messagebus (RabbitMQ) - Message broker
4. identity (Keycloak) - Depends on database
5. eshop-api (Main App) - Depends on all above
6. eshop-pgadmin (Admin UI) - Depends on database
7. eshop-seq (Logging) - Independent
```

## ?? **Expected Results After Cleanup**

After running the cleanup script, you should see:

### **? Successful Service Status**
```
     Name                   Command               State                    Ports
eshop-application   dotnet Api.dll                Up      0.0.0.0:8080->8080/tcp, 0.0.0.0:8081->8081/tcp
eshop-database      docker-entrypoint.sh postgres Up      0.0.0.0:5432->5432/tcp
eshop-identity      /opt/keycloak/bin/kc.sh st... Up      0.0.0.0:8082->8080/tcp, 0.0.0.0:8443->8443/tcp
eshop-messagebus    docker-entrypoint.sh rabbitmq Up      15671/tcp, 0.0.0.0:15672->15672/tcp, 25672/tcp, 4369/tcp, 5671/tcp, 0.0.0.0:5672->5672/tcp
eshop-pgadmin       /entrypoint.sh                 Up      443/tcp, 0.0.0.0:5050->80/tcp
eshop-redis         docker-entrypoint.sh redis ... Up      0.0.0.0:6379->6379/tcp
eshop-seq           ./run.sh                       Up      0.0.0.0:5341->80/tcp
```

### **?? Accessible Endpoints**
- **API Health**: http://localhost:8080/health ?
- **Keycloak Admin**: http://localhost:8082 ?
- **pgAdmin**: http://localhost:5050 ?
- **All other services**: Running and accessible ?

## ?? **Additional Resources**

- **Docker Compose Documentation**: https://docs.docker.com/compose/
- **Container Lifecycle Management**: https://docs.docker.com/engine/reference/commandline/container/
- **eShop Architecture**: Check your project's README.md

Your .NET 8 eShop Modular Monolith should now be running cleanly without any container conflicts! ??