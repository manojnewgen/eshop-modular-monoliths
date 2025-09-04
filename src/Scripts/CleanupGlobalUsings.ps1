# PowerShell script to clean up redundant using statements after implementing Global Usings
# Run this script from the solution root directory

# Define global usings that should be removed from individual files
$GlobalUsings = @(
    "using System;",
    "using System.Collections.Generic;",
    "using System.Linq;",
    "using System.Text;",
    "using System.Threading;",
    "using System.Threading.Tasks;",
    "using Microsoft.Extensions.DependencyInjection;",
    "using Microsoft.Extensions.Logging;",
    "using Microsoft.Extensions.Configuration;",
    "using Microsoft.AspNetCore.Builder;",
    "using Microsoft.AspNetCore.Http;",
    "using Microsoft.AspNetCore.Routing;",
    "using Microsoft.EntityFrameworkCore;",
    "using MediatR;",
    "using Mapster;",
    "using Carter;",
    "using Shared.DDD;",
    "using Shared.CQRS;",
    "using Shared.Mapping;",
    "using Catalog.Data;",
    "using Catalog.Products.Dtos;",
    "using Catalog.Products.Events;"
)

# Function to clean up a single file
function CleanupFile {
    param (
        [string]$FilePath
    )
    
    Write-Host "Processing: $FilePath"
    
    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    
    # Remove global usings from the file
    foreach ($using in $GlobalUsings) {
        $content = $content -replace [regex]::Escape($using), ""
    }
    
    # Clean up empty lines at the top
    $content = $content -replace "^(\r?\n)+", ""
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content $FilePath $content -NoNewline
        Write-Host "  ? Cleaned up: $FilePath"
    } else {
        Write-Host "  ?? No changes needed: $FilePath"
    }
}

# Get all C# files in the Catalog module
$catalogFiles = Get-ChildItem -Path "Modules\Catalog\Catalog\*.cs" -Recurse | Where-Object { $_.Name -ne "GlobalUsings.cs" }

Write-Host "?? Starting cleanup of Catalog module files..."
Write-Host "Found $($catalogFiles.Count) C# files to process"
Write-Host ""

foreach ($file in $catalogFiles) {
    CleanupFile $file.FullName
}

Write-Host ""
Write-Host "? Cleanup completed!"
Write-Host ""
Write-Host "?? Next steps:"
Write-Host "1. Build the solution to verify everything still works"
Write-Host "2. Run tests to ensure functionality is preserved"
Write-Host "3. Repeat this process for other modules (Basket, Ordering, Api)"
Write-Host ""
Write-Host "?? Benefits achieved:"
Write-Host "   • Reduced code duplication"
Write-Host "   • Cleaner, more focused files"
Write-Host "   • Centralized dependency management"
Write-Host "   • Easier maintenance and refactoring"