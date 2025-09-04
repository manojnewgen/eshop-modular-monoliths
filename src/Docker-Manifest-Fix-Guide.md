# ?? Docker Linux/AMD64 Manifest Error Fix

## Issue: "no matching manifest for linux/amd64 in the manifest list entries"

This error typically occurs when:
- Docker registry connectivity issues
- Image registry temporary unavailability  
- Platform/architecture mismatch
- Corrupted Docker image cache

## ? **Step-by-Step Fix**

### 1. **Restart Docker Desktop**
```powershell
# Close Docker Desktop completely
# Right-click Docker Desktop in system tray ? Quit Docker Desktop
# Wait 30 seconds
# Start Docker Desktop again
# Wait for "Docker Desktop is running" message
```

### 2. **Clear Docker Cache and Images**
```powershell
# Stop all containers and remove images
docker-compose down --rmi all

# Remove all unused images, containers, networks, and build cache
docker system prune -a -f

# Remove all volumes (optional - will delete data)
docker volume prune -f

# Clear buildkit cache
docker builder prune -a -f
```

### 3. **Verify Docker Platform**
```powershell
# Check Docker version and platform
docker version

# Should show:
# Server: Docker Desktop
#  OS/Arch: linux/amd64
```

### 4. **Test Individual Image Pulls**
```powershell
# Test pulling .NET images
docker pull mcr.microsoft.com/dotnet/aspnet:8.0
docker pull mcr.microsoft.com/dotnet/sdk:8.0

# Test pulling Alpine-based images
docker pull postgres:15-alpine
docker pull redis:7-alpine

# Test pulling other images
docker pull dpage/pgadmin4:latest
docker pull datalust/seq:latest
```

### 5. **Check Image Manifests**
```powershell
# Check available platforms for .NET images
docker manifest inspect mcr.microsoft.com/dotnet/aspnet:8.0
docker manifest inspect mcr.microsoft.com/dotnet/sdk:8.0

# Check PostgreSQL platforms
docker manifest inspect postgres:15-alpine
```

## ?? **Alternative Image Versions**

If specific images are having issues, try alternative tags:

### **.NET Images**
```yaml
# In Dockerfile, try different tags:
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
# OR
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
# OR  
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
```

### **Database Images**
```yaml
# In docker-compose.yml, try:
eshop-db:
  image: postgres:15  # Remove -alpine if having issues
  # OR
  image: postgres:15.3-alpine3.18
```

### **Redis Images**
```yaml
eshop-redis:
  image: redis:7  # Remove -alpine if having issues
  # OR
  image: redis:7.2-alpine3.18
```

## ??? **Troubleshooting Commands**

### **Check Docker Configuration**
```powershell
# Check Docker info
docker info

# Check if Docker is using Linux containers
docker version | findstr "OS/Arch"
```

### **Check Network Connectivity**
```powershell
# Test connectivity to Microsoft Container Registry
nslookup mcr.microsoft.com
ping mcr.microsoft.com

# Test Docker Hub connectivity  
nslookup docker.io
ping registry-1.docker.io
```

### **Force Platform Specification**
```powershell
# Pull with explicit platform
docker pull --platform linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0
docker pull --platform linux/amd64 postgres:15-alpine
```

### **Docker Desktop Settings Check**
```
1. Open Docker Desktop
2. Go to Settings (gear icon)
3. General tab:
   ?? Use the WSL 2 based engine
   ?? Use Docker Compose V2
4. Resources ? Advanced:
   - Memory: At least 4GB
   - CPUs: At least 2
```

## ?? **Working Image Combinations**

If you continue having issues, use these tested combinations:

```yaml
# docker-compose.yml - Stable image versions
services:
  eshop-db:
    image: postgres:15.3
    platform: linux/amd64
    
  eshop-redis:
    image: redis:7.0
    platform: linux/amd64
    
  eshop-pgadmin:
    image: dpage/pgadmin4:7.8
    platform: linux/amd64
    
  eshop-seq:
    image: datalust/seq:2023.4
    platform: linux/amd64
```

```dockerfile
# Dockerfile - Stable .NET versions
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
```

## ?? **Complete Reset Process**

If all else fails, perform a complete Docker reset:

```powershell
# 1. Stop everything
docker-compose down -v --remove-orphans

# 2. Remove everything
docker system prune -a --volumes -f
docker builder prune -a -f

# 3. Reset Docker Desktop to factory defaults
# Docker Desktop ? Settings ? Troubleshoot ? Reset to factory defaults

# 4. Restart computer (if needed)

# 5. Start fresh
docker-compose up --build -d
```

## ?? **Prevention Tips**

1. **Pin image versions** instead of using `latest`
2. **Use stable base images** like `jammy` or `bookworm-slim`
3. **Monitor Docker Hub status** at status.docker.com
4. **Keep Docker Desktop updated**
5. **Use multi-platform builds** for CI/CD