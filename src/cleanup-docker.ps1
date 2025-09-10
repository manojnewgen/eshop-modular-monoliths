# Enhanced Docker Cleanup Script for eShop Modular Monolith
# Resolves container naming conflicts and provides comprehensive restart

Write-Host "?? eShop Docker Cleanup & Restart Script - Enhanced" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan

# Function to check if Docker is running
function Test-DockerRunning {
    try {
        docker version 2>$null | Out-Null
        return $true
    }
    catch {
        Write-Host "? Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
        return $false
    }
}

# Function to remove container with status feedback
function Remove-ContainerSafe {
    param([string]$ContainerName)
    
    try {
        $result = docker container rm -f $ContainerName 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ? Removed $ContainerName" -ForegroundColor Green
        } else {
            Write-Host "  ??  $ContainerName not found (already removed)" -ForegroundColor Blue
        }
    }
    catch {
        Write-Host "  ??  Could not remove $ContainerName" -ForegroundColor Yellow
    }
}

# Check Docker status
Write-Host "`n?? Checking Docker status..." -ForegroundColor Yellow
if (-not (Test-DockerRunning)) {
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "? Docker is running" -ForegroundColor Green

# Step 1: Stop all compose services
Write-Host "`n?? Stopping Docker Compose services..." -ForegroundColor Yellow
try {
    docker-compose down --remove-orphans 2>$null
    Write-Host "? Stopped compose services" -ForegroundColor Green
}
catch {
    Write-Host "?? No compose services were running or compose file not found" -ForegroundColor Yellow
}

# Step 2: Remove specific eShop containers with detailed feedback
Write-Host "`n??? Removing eShop containers..." -ForegroundColor Yellow
$containerNames = @(
    "eshop-seq",
    "eshop-database", 
    "eshop-redis",
    "eshop-messagebus",
    "eshop-identity",
    "eshop-pgadmin",
    "eshop-application"
)

foreach ($container in $containerNames) {
    Remove-ContainerSafe -ContainerName $container
}

# Step 3: Clean up orphaned resources
Write-Host "`n?? Cleaning up orphaned Docker resources..." -ForegroundColor Yellow
try {
    Write-Host "  Pruning containers..." -ForegroundColor Cyan
    docker container prune -f 2>$null | Out-Null
    Write-Host "  Pruning networks..." -ForegroundColor Cyan
    docker network prune -f 2>$null | Out-Null
    Write-Host "? Cleaned up orphaned resources" -ForegroundColor Green
}
catch {
    Write-Host "?? Some cleanup operations may have failed" -ForegroundColor Yellow
}

# Step 4: Show current Docker status
Write-Host "`n?? Current Docker container status:" -ForegroundColor Cyan
$runningContainers = docker ps -a --filter "name=eshop-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>$null
if ($runningContainers -and $runningContainers.Count -gt 1) {
    Write-Host $runningContainers
} else {
    Write-Host "  No eShop containers currently exist" -ForegroundColor Blue
}

# Step 5: Validate Docker Compose configuration
Write-Host "`n?? Validating Docker Compose configuration..." -ForegroundColor Yellow
try {
    docker-compose config --quiet 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Docker Compose configuration is valid" -ForegroundColor Green
    } else {
        Write-Host "? Docker Compose configuration has errors" -ForegroundColor Red
        Write-Host "Please check docker-compose.yml file for syntax errors" -ForegroundColor Yellow
        Read-Host "Press Enter to continue anyway or Ctrl+C to exit"
    }
}
catch {
    Write-Host "?? Could not validate compose configuration" -ForegroundColor Yellow
}

# Step 6: Start fresh services
Write-Host "`n?? Starting eShop services..." -ForegroundColor Yellow
try {
    docker-compose up -d
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Started eShop services successfully!" -ForegroundColor Green
    } else {
        Write-Host "? Failed to start services. Check the error output above." -ForegroundColor Red
        Read-Host "Press Enter to continue"
        exit 1
    }
} catch {
    Write-Host "? Exception occurred while starting services" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 7: Wait and show service status
Write-Host "`n? Waiting for services to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`n?? Final Service Status:" -ForegroundColor Cyan
try {
    docker-compose ps
} catch {
    Write-Host "Could not retrieve service status" -ForegroundColor Yellow
}

# Step 8: Health check
Write-Host "`n?? Performing basic health checks..." -ForegroundColor Yellow
$healthChecks = @{
    "API Health" = "http://localhost:8080/health"
    "pgAdmin" = "http://localhost:5050"
    "Keycloak" = "http://localhost:8082"
}

foreach ($service in $healthChecks.GetEnumerator()) {
    try {
        $response = Invoke-WebRequest -Uri $service.Value -Method Get -TimeoutSec 5 -UseBasicParsing 2>$null
        if ($response.StatusCode -eq 200) {
            Write-Host "  ? $($service.Key) is responding" -ForegroundColor Green
        } else {
            Write-Host "  ?? $($service.Key) returned status $($response.StatusCode)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "  ? $($service.Key) is still starting up or not accessible" -ForegroundColor Blue
    }
}

# Step 9: Show helpful endpoints and commands
Write-Host "`n?? Available Endpoints:" -ForegroundColor Cyan
Write-Host "  • API (HTTP):           http://localhost:8080" -ForegroundColor White
Write-Host "  • API (HTTPS):          https://localhost:8081" -ForegroundColor White  
Write-Host "  • Health Check:         http://localhost:8080/health" -ForegroundColor White
Write-Host "  • Swagger (Dev):        http://localhost:8080/swagger" -ForegroundColor White
Write-Host "  • API Info:             http://localhost:8080/" -ForegroundColor White
Write-Host "  • pgAdmin:              http://localhost:5050 (admin@eshop.com / admin123)" -ForegroundColor White
Write-Host "  • Keycloak Admin:       http://localhost:8082 (admin / admin123)" -ForegroundColor White
Write-Host "  • RabbitMQ Management:  http://localhost:15672 (guest / guest)" -ForegroundColor White
Write-Host "  • Seq Logging:          http://localhost:5341" -ForegroundColor White

Write-Host "`n?? Useful Commands:" -ForegroundColor Cyan
Write-Host "  • View all logs:        docker-compose logs" -ForegroundColor Gray
Write-Host "  • View API logs:        docker-compose logs -f eshop-api" -ForegroundColor Gray
Write-Host "  • View identity logs:   docker-compose logs -f identity" -ForegroundColor Gray
Write-Host "  • Stop all services:    docker-compose down" -ForegroundColor Gray
Write-Host "  • Restart service:      docker-compose restart <service-name>" -ForegroundColor Gray
Write-Host "  • Check service status: docker-compose ps" -ForegroundColor Gray

Write-Host "`n?? Docker cleanup and restart completed!" -ForegroundColor Green
Write-Host "?? If services are still starting, wait a few minutes before accessing endpoints" -ForegroundColor Blue

Read-Host "`nPress Enter to exit"