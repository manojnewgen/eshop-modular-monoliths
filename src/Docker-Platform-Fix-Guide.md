# ?? Docker Platform Configuration Guide

## Issue: Platform Manifest Error
The error "no matching manifest for windows(10.0.26100)/amd64" occurs when there's a mismatch between:
- Docker container platform (Windows vs Linux)
- Base image architecture (AMD64, ARM64, etc.)

## ? Solution Steps

### 1. **Configure Docker Desktop for Linux Containers**
```powershell
# Ensure Docker Desktop is running in Linux mode
# Right-click Docker Desktop system tray icon ? "Switch to Linux containers"
```

### 2. **Verify Docker Platform**
```bash
# Check current Docker platform
docker version

# Should show:
# Server: Docker Desktop
#  Engine:
#   Version: [version]
#   OS/Arch: linux/amd64
```

### 3. **Build with Explicit Platform (if needed)**
```bash
# Build for specific platform
docker build --platform linux/amd64 -f Bootstrapper/Api/Dockerfile .

# Or for multi-platform
docker build --platform linux/amd64,linux/arm64 -f Bootstrapper/Api/Dockerfile .
```

### 4. **Docker Compose with Platform Specification**
```yaml
# Add platform specification to docker-compose.yml if needed
services:
  eshop-api:
    build:
      context: .
      dockerfile: Bootstrapper/Api/Dockerfile
    platform: linux/amd64  # Explicit platform
```

### 5. **Verify Image Compatibility**
```bash
# Check available platforms for an image
docker manifest inspect mcr.microsoft.com/dotnet/aspnet:8.0

# Pull specific platform
docker pull --platform linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0
```

## ?? **Recommended Configuration**

### Project Settings
- **DockerDefaultTargetOS**: Linux
- **Base Images**: Linux-based (.NET 8 images)
- **Docker Desktop**: Linux container mode

### Development Workflow
```bash
# 1. Clean any existing containers/images
docker-compose down --rmi all
docker system prune -f

# 2. Build and start fresh
docker-compose up --build -d

# 3. Monitor logs
docker-compose logs -f eshop-api
```

### Troubleshooting Commands
```bash
# Check container platform
docker inspect eshop-application | grep -i platform

# Check image architecture
docker image inspect eshop-api:latest | grep -i architecture

# Force rebuild without cache
docker-compose build --no-cache eshop-api
```

## ?? **Platform Compatibility Matrix**

| Host OS | Docker Mode | .NET Images | Status |
|---------|-------------|-------------|---------|
| Windows | Linux Containers | linux/amd64 | ? Recommended |
| Windows | Windows Containers | windows/amd64 | ?? Limited ecosystem |
| Linux | Native | linux/amd64 | ? Optimal |
| macOS | Linux Containers | linux/amd64 | ? Good |

## ?? **Migration from Windows to Linux Containers**

If you were previously using Windows containers:

1. **Update Dockerfile**: Use Linux base images
2. **Update Project**: Set `DockerDefaultTargetOS` to Linux
3. **Test Dependencies**: Ensure all NuGet packages support Linux
4. **Update Scripts**: Use Linux-style commands in scripts

## ?? **Best Practices**

1. **Use Linux containers** for better ecosystem compatibility
2. **Specify platforms explicitly** in CI/CD pipelines
3. **Use multi-stage builds** for optimal image size
4. **Pin image versions** for reproducible builds
5. **Test on target platform** before deployment