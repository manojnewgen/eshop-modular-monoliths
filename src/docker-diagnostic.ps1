# eShop Docker Manifest Diagnostic Script for Windows
# Run this to identify the exact cause of manifest errors

Write-Host "?? eShop Docker Manifest Diagnostic" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Check Docker Desktop status
Write-Host "1. Checking Docker Desktop Status..." -ForegroundColor Yellow
try {
    $dockerVersion = docker version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Docker is running" -ForegroundColor Green
        docker version | Select-String "Version|OS/Arch"
    } else {
        Write-Host "? Docker is not running or not accessible" -ForegroundColor Red
        Write-Host "   ? Start Docker Desktop and try again" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "? Docker command not found" -ForegroundColor Red
    Write-Host "   ? Install Docker Desktop" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Check Docker platform
Write-Host "2. Checking Docker Platform..." -ForegroundColor Yellow
try {
    $dockerInfo = docker info --format "{{.OSType}}/{{.Architecture}}" 2>$null
    Write-Host "   Platform: $dockerInfo" -ForegroundColor White
    if ($dockerInfo -eq "linux/x86_64" -or $dockerInfo -eq "linux/amd64") {
        Write-Host "? Platform is correct (linux/amd64)" -ForegroundColor Green
    } else {
        Write-Host "??  Platform might cause issues: $dockerInfo" -ForegroundColor Yellow
        Write-Host "   ? Ensure Docker Desktop is in Linux container mode" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? Cannot determine Docker platform" -ForegroundColor Red
}
Write-Host ""

# Check network connectivity
Write-Host "3. Checking Network Connectivity..." -ForegroundColor Yellow
try {
    $mcrTest = Test-NetConnection mcr.microsoft.com -Port 443 -InformationLevel Quiet -WarningAction SilentlyContinue
    if ($mcrTest) {
        Write-Host "? Microsoft Container Registry accessible" -ForegroundColor Green
    } else {
        Write-Host "? Cannot reach Microsoft Container Registry" -ForegroundColor Red
        Write-Host "   ? Check internet connection or corporate firewall" -ForegroundColor Yellow
    }
} catch {
    Write-Host "??  Network test failed for MCR" -ForegroundColor Yellow
}

try {
    $dockerHubTest = Test-NetConnection registry-1.docker.io -Port 443 -InformationLevel Quiet -WarningAction SilentlyContinue
    if ($dockerHubTest) {
        Write-Host "? Docker Hub accessible" -ForegroundColor Green
    } else {
        Write-Host "? Cannot reach Docker Hub" -ForegroundColor Red
        Write-Host "   ? Check internet connection or corporate firewall" -ForegroundColor Yellow
    }
} catch {
    Write-Host "??  Network test failed for Docker Hub" -ForegroundColor Yellow
}
Write-Host ""

# Test critical images
Write-Host "4. Testing Critical Images..." -ForegroundColor Yellow
$images = @(
    "mcr.microsoft.com/dotnet/aspnet:8.0-jammy",
    "mcr.microsoft.com/dotnet/sdk:8.0-jammy",
    "postgres:15.3",
    "redis:7.0",
    "dpage/pgadmin4:7.8",
    "datalust/seq:2023.4"
)

foreach ($image in $images) {
    Write-Host "   Testing: $image" -ForegroundColor White
    try {
        $manifestCheck = docker manifest inspect $image 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ? Manifest available" -ForegroundColor Green
        } else {
            Write-Host "   ? Manifest not available" -ForegroundColor Red
            Write-Host "      ? This image is causing the manifest error" -ForegroundColor Yellow
            
            # Suggest alternatives
            switch -Wildcard ($image) {
                "*dotnet/aspnet*" { 
                    Write-Host "      ? Try: mcr.microsoft.com/dotnet/aspnet:8.0" -ForegroundColor Cyan 
                }
                "*dotnet/sdk*" { 
                    Write-Host "      ? Try: mcr.microsoft.com/dotnet/sdk:8.0" -ForegroundColor Cyan 
                }
                "*postgres*" { 
                    Write-Host "      ? Try: postgres:15 or postgres:15-alpine" -ForegroundColor Cyan 
                }
                "*redis*" { 
                    Write-Host "      ? Try: redis:7 or redis:7-alpine" -ForegroundColor Cyan 
                }
                "*pgadmin*" { 
                    Write-Host "      ? Try: dpage/pgadmin4:latest" -ForegroundColor Cyan 
                }
                "*seq*" { 
                    Write-Host "      ? Try: datalust/seq:latest" -ForegroundColor Cyan 
                }
            }
        }
    } catch {
        Write-Host "   ? Error checking manifest" -ForegroundColor Red
    }
}
Write-Host ""

# Check workspace-specific files
Write-Host "5. Checking Workspace Configuration..." -ForegroundColor Yellow
if (Test-Path "docker-compose.yml") {
    Write-Host "? docker-compose.yml found" -ForegroundColor Green
    
    # Check for platform specifications
    $composeContent = Get-Content "docker-compose.yml" -Raw
    if ($composeContent -match "platform:") {
        Write-Host "? Platform specifications found in docker-compose.yml" -ForegroundColor Green
        (Get-Content "docker-compose.yml" | Select-String "platform:") | Select-Object -First 3
    } else {
        Write-Host "??  No platform specifications in docker-compose.yml" -ForegroundColor Yellow
        Write-Host "   ? Consider adding 'platform: linux/amd64' to services" -ForegroundColor Yellow
    }
} else {
    Write-Host "? docker-compose.yml not found" -ForegroundColor Red
    Write-Host "   ? Run this script from the project root directory" -ForegroundColor Yellow
    Write-Host "   ? Expected location: C:\Manoj\eshop-modular-monoliths\src\" -ForegroundColor Yellow
}

if (Test-Path "Bootstrapper\Api\Dockerfile") {
    Write-Host "? Dockerfile found" -ForegroundColor Green
    
    # Check base images
    Write-Host "   Base images:" -ForegroundColor White
    Get-Content "Bootstrapper\Api\Dockerfile" | Select-String "FROM"
} else {
    Write-Host "? Dockerfile not found at Bootstrapper\Api\Dockerfile" -ForegroundColor Red
}
Write-Host ""

# Check Docker resources
Write-Host "6. Checking Docker Resources..." -ForegroundColor Yellow
try {
    $dockerInfo = docker info 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Docker info accessible" -ForegroundColor Green
        $dockerInfo | Select-String "Total Memory|CPUs"
        
        # Check for WSL2 (on Windows)
        if ($dockerInfo | Select-String "WSL") {
            Write-Host "? WSL2 backend detected" -ForegroundColor Green
        }
    } else {
        Write-Host "? Cannot get Docker info" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error getting Docker info" -ForegroundColor Red
}
Write-Host ""

# Check Visual Studio integration
Write-Host "7. Checking Visual Studio Integration..." -ForegroundColor Yellow
if (Test-Path "docker-compose.dcproj") {
    Write-Host "? Docker Compose project found" -ForegroundColor Green
} else {
    Write-Host "??  No Docker Compose project file" -ForegroundColor Yellow
}

# Check for VS generated files
$vsDockerFiles = Get-ChildItem -Path "." -Filter "*docker-compose.vs.*" -ErrorAction SilentlyContinue
if ($vsDockerFiles) {
    Write-Host "??  Visual Studio Docker files detected:" -ForegroundColor Yellow
    $vsDockerFiles | ForEach-Object { Write-Host "   $($_.Name)" -ForegroundColor White }
    Write-Host "   ? These might need to be cleaned" -ForegroundColor Yellow
}
Write-Host ""

# Recommendations
Write-Host "?? Recommendations:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

$hasIssues = $false

if ($LASTEXITCODE -ne 0) {
    Write-Host "1. ? Start Docker Desktop" -ForegroundColor Red
    $hasIssues = $true
}

if ($dockerInfo -notmatch "linux") {
    Write-Host "2. ? Switch Docker Desktop to Linux containers" -ForegroundColor Red
    Write-Host "   ? Right-click Docker Desktop tray icon ? 'Switch to Linux containers'" -ForegroundColor Yellow
    $hasIssues = $true
}

if (-not $mcrTest) {
    Write-Host "3. ? Fix network connectivity to container registries" -ForegroundColor Red
    Write-Host "   ? Check firewall/proxy settings" -ForegroundColor Yellow
    $hasIssues = $true
}

Write-Host ""
Write-Host "4. ?? Try these commands to fix:" -ForegroundColor Cyan
Write-Host "   docker-compose down --rmi all" -ForegroundColor White
Write-Host "   docker system prune -a -f" -ForegroundColor White
Write-Host "   docker builder prune -a -f" -ForegroundColor White
Write-Host "   docker-compose up --build -d" -ForegroundColor White
Write-Host ""

Write-Host "5. ?? Ensure you're in the correct directory:" -ForegroundColor Cyan
Write-Host "   cd 'C:\Manoj\eshop-modular-monoliths\src'" -ForegroundColor White
Write-Host ""

if ($vsDockerFiles) {
    Write-Host "6. ?? Clean Visual Studio Docker files:" -ForegroundColor Cyan
    Write-Host "   Remove-Item '*docker-compose.vs.*' -Force" -ForegroundColor White
    Write-Host "   Remove-Item 'obj\Docker' -Recurse -Force -ErrorAction SilentlyContinue" -ForegroundColor White
    Write-Host ""
}

Write-Host "?? Diagnostic Complete!" -ForegroundColor Green
if ($hasIssues) {
    Write-Host "??  Issues detected - follow the recommendations above" -ForegroundColor Yellow
} else {
    Write-Host "? No major issues detected - try rebuilding your containers" -ForegroundColor Green
}
Write-Host "?? For detailed troubleshooting, check Visual-Studio-Docker-Fix.md" -ForegroundColor Cyan