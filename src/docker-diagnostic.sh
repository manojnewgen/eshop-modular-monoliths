#!/bin/bash

# eShop Docker Manifest Diagnostic Script
# Run this to identify the exact cause of manifest errors

echo "?? eShop Docker Manifest Diagnostic"
echo "===================================="
echo ""

# Check Docker Desktop status
echo "1. Checking Docker Desktop Status..."
if docker version > /dev/null 2>&1; then
    echo "? Docker is running"
    docker version | grep -E "Version|OS/Arch"
else
    echo "? Docker is not running or not accessible"
    echo "   ? Start Docker Desktop and try again"
    exit 1
fi
echo ""

# Check Docker platform
echo "2. Checking Docker Platform..."
DOCKER_PLATFORM=$(docker version --format '{{.Server.Os}}/{{.Server.Arch}}')
echo "   Platform: $DOCKER_PLATFORM"
if [[ "$DOCKER_PLATFORM" == "linux/amd64" ]]; then
    echo "? Platform is correct (linux/amd64)"
else
    echo "??  Platform might cause issues"
    echo "   ? Ensure Docker Desktop is in Linux container mode"
fi
echo ""

# Check network connectivity
echo "3. Checking Network Connectivity..."
if curl -s --connect-timeout 5 https://mcr.microsoft.com > /dev/null; then
    echo "? Microsoft Container Registry accessible"
else
    echo "? Cannot reach Microsoft Container Registry"
    echo "   ? Check internet connection or corporate firewall"
fi

if curl -s --connect-timeout 5 https://registry-1.docker.io > /dev/null; then
    echo "? Docker Hub accessible"
else
    echo "? Cannot reach Docker Hub"
    echo "   ? Check internet connection or corporate firewall"
fi
echo ""

# Test critical images
echo "4. Testing Critical Images..."
IMAGES=(
    "mcr.microsoft.com/dotnet/aspnet:8.0-jammy"
    "mcr.microsoft.com/dotnet/sdk:8.0-jammy"
    "postgres:15.3"
    "redis:7.0"
)

for image in "${IMAGES[@]}"; do
    echo "   Testing: $image"
    if docker manifest inspect "$image" > /dev/null 2>&1; then
        echo "   ? Manifest available"
    else
        echo "   ? Manifest not available"
        echo "      ? This image is causing the manifest error"
        
        # Try alternative
        case $image in
            *dotnet/aspnet*)
                echo "      ? Try: mcr.microsoft.com/dotnet/aspnet:8.0"
                ;;
            *dotnet/sdk*)
                echo "      ? Try: mcr.microsoft.com/dotnet/sdk:8.0"
                ;;
            *postgres*)
                echo "      ? Try: postgres:15 or postgres:15-alpine"
                ;;
            *redis*)
                echo "      ? Try: redis:7 or redis:7-alpine"
                ;;
        esac
    fi
done
echo ""

# Check workspace-specific files
echo "5. Checking Workspace Configuration..."
if [[ -f "docker-compose.yml" ]]; then
    echo "? docker-compose.yml found"
    
    # Check for platform specifications
    if grep -q "platform:" docker-compose.yml; then
        echo "? Platform specifications found in docker-compose.yml"
        grep "platform:" docker-compose.yml | head -3
    else
        echo "??  No platform specifications in docker-compose.yml"
        echo "   ? Consider adding 'platform: linux/amd64' to services"
    fi
else
    echo "? docker-compose.yml not found"
    echo "   ? Run this script from the project root directory"
fi

if [[ -f "Bootstrapper/Api/Dockerfile" ]]; then
    echo "? Dockerfile found"
    
    # Check base images
    echo "   Base images:"
    grep "FROM" Bootstrapper/Api/Dockerfile
else
    echo "? Dockerfile not found at Bootstrapper/Api/Dockerfile"
fi
echo ""

# Check Docker resources
echo "6. Checking Docker Resources..."
DOCKER_INFO=$(docker info 2>/dev/null)
if [[ $? -eq 0 ]]; then
    echo "? Docker info accessible"
    echo "$DOCKER_INFO" | grep -E "Total Memory|CPUs"
    
    # Check for WSL2 (on Windows)
    if echo "$DOCKER_INFO" | grep -q "WSL"; then
        echo "? WSL2 backend detected"
    fi
else
    echo "? Cannot get Docker info"
fi
echo ""

# Recommendations
echo "?? Recommendations:"
echo "=================="

if ! docker version > /dev/null 2>&1; then
    echo "1. ? Start Docker Desktop"
fi

if [[ "$DOCKER_PLATFORM" != "linux/amd64" ]]; then
    echo "2. ? Switch Docker Desktop to Linux containers"
    echo "   ? Right-click Docker Desktop tray icon ? 'Switch to Linux containers'"
fi

if ! curl -s --connect-timeout 5 https://mcr.microsoft.com > /dev/null; then
    echo "3. ? Fix network connectivity to container registries"
    echo "   ? Check firewall/proxy settings"
fi

echo "4. ?? Try these commands to fix:"
echo "   docker-compose down --rmi all"
echo "   docker system prune -a -f"
echo "   docker-compose up --build -d"
echo ""

echo "5. ?? Ensure you're in the correct directory:"
echo "   cd C:\\Manoj\\eshop-modular-monoliths\\src"
echo ""

echo "?? Diagnostic Complete!"
echo "If issues persist, check the Visual-Studio-Docker-Fix.md guide"