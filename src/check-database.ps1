# PowerShell script to check PostgreSQL database
# Save this as check-database.ps1

Write-Host "?? Checking PostgreSQL Database Schemas and Tables" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Test connection
Write-Host "`n1. Testing connection to PostgreSQL..." -ForegroundColor Yellow
try {
    $env:PGPASSWORD = "postgres"
    $result = & psql -h localhost -p 5432 -U postgres -d eshopdb -c "SELECT current_database();" 2>$null
    if ($result -match "eshopdb") {
        Write-Host "   ? Connected to database: eshopdb" -ForegroundColor Green
    } else {
        Write-Host "   ??  Connected to default database, trying to create eshopdb..." -ForegroundColor Yellow
        & psql -h localhost -p 5432 -U postgres -c "CREATE DATABASE eshopdb;" 2>$null
    }
} catch {
    Write-Host "   ? Cannot connect to PostgreSQL. Make sure it's running on localhost:5432" -ForegroundColor Red
    Write-Host "   Credentials: username=postgres, password=postgres" -ForegroundColor Red
    exit 1
}

# Check schemas
Write-Host "`n2. Checking database schemas..." -ForegroundColor Yellow
$schemas = & psql -h localhost -p 5432 -U postgres -d eshopdb -t -c "SELECT schema_name FROM information_schema.schemata WHERE schema_name IN ('catalog', 'basket', 'ordering', 'shared', 'messaging', 'identity') ORDER BY schema_name;" 2>$null

if ($schemas) {
    Write-Host "   ?? Found schemas:" -ForegroundColor Green
    $schemas | ForEach-Object { 
        $schema = $_.Trim()
        if ($schema) { Write-Host "      ? $schema" -ForegroundColor Green }
    }
} else {
    Write-Host "   ??  No custom schemas found. Migration may not have run yet." -ForegroundColor Yellow
}

# Check tables in each schema
$schemaList = @('catalog', 'basket', 'ordering', 'shared', 'messaging', 'identity')

foreach ($schema in $schemaList) {
    Write-Host "`n3. Checking tables in '$schema' schema..." -ForegroundColor Yellow
    
    $tables = & psql -h localhost -p 5432 -U postgres -d eshopdb -t -c "SELECT table_name FROM information_schema.tables WHERE table_schema = '$schema' ORDER BY table_name;" 2>$null
    
    if ($tables) {
        Write-Host "   ?? Tables in $schema schema:" -ForegroundColor Green
        $tables | ForEach-Object { 
            $table = $_.Trim()
            if ($table) { Write-Host "      ? $table" -ForegroundColor Green }
        }
    } else {
        Write-Host "   ?? No tables found in '$schema' schema (may not be migrated yet)" -ForegroundColor Gray
    }
}

# Check migration history
Write-Host "`n4. Checking Entity Framework migration history..." -ForegroundColor Yellow
$migrationTables = & psql -h localhost -p 5432 -U postgres -d eshopdb -t -c "SELECT schemaname || '.' || tablename as full_name FROM pg_tables WHERE tablename = '__EFMigrationsHistory';" 2>$null

if ($migrationTables) {
    Write-Host "   ?? Migration history tables found:" -ForegroundColor Green
    $migrationTables | ForEach-Object { 
        $table = $_.Trim()
        if ($table) { Write-Host "      ? $table" -ForegroundColor Green }
    }
} else {
    Write-Host "   ??  No migration history found. Migrations haven't run yet." -ForegroundColor Yellow
}

Write-Host "`n?? Database check completed!" -ForegroundColor Green