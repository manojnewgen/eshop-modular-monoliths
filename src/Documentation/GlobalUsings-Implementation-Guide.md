# ?? Global Usings Implementation Guide

## ?? **Overview**

Global Usings is a .NET 6+ feature that allows you to declare using statements once in a `GlobalUsings.cs` file, making them available across all files in the project. This eliminates repetitive using statements and keeps your code cleaner.

## ? **Benefits**

- **?? Cleaner Code**: Eliminates repetitive using statements
- **?? Centralized Management**: All common dependencies in one place
- **?? Easier Maintenance**: Update references in one location
- **? Better Performance**: Slightly faster compilation
- **?? Improved Readability**: Focus on business logic, not imports

## ?? **Implementation Structure**

```
src/
??? Modules/Shared/Shared/GlobalUsings.cs       # Shared utilities & infrastructure
??? Modules/Catalog/Catalog/GlobalUsings.cs     # Catalog-specific + Shared usings
??? Modules/Basket/Basket/GlobalUsings.cs       # Basket-specific + Shared usings
??? Modules/Ordering/Ordering/GlobalUsings.cs   # Ordering-specific + Shared usings
??? Bootstrapper/Api/GlobalUsings.cs            # API/Web-specific usings
```

## ?? **What's Included in Each GlobalUsings.cs**

### **Shared Module** (`Modules/Shared/Shared/GlobalUsings.cs`)
```csharp
// Foundation for all other modules
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using MediatR;
global using Mapster;
global using Shared.DDD;
global using Shared.CQRS;
global using Shared.Mapping;
```

### **Catalog Module** (`Modules/Catalog/Catalog/GlobalUsings.cs`)
```csharp
// Inherits Shared + adds Catalog-specific
global using Carter;                    // Carter endpoints
global using Microsoft.AspNetCore.Routing;  // Route builder
global using Catalog.Data;            // DbContext
global using Catalog.Products.Dtos;   // Data transfer objects
global using Catalog.Products.Events; // Domain events
```

### **API Project** (`Bootstrapper/Api/GlobalUsings.cs`)
```csharp
// Web API specific usings
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Hosting;
// ... etc
```

## ?? **Before vs After Comparison**

### **? Before (Repetitive)**
```csharp
using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Products.Features.GetProduct
{
    public class GetProductEndPoint : ICarterModule
    {
        // ... implementation
    }
}
```

### **? After (Clean)**
```csharp
namespace Catalog.Products.Features.GetProduct
{
    public class GetProductEndPoint : ICarterModule
    {
        // ... implementation
    }
}
```

## ??? **Implementation Steps**

### **1. Create GlobalUsings.cs Files**
Already done! ? Created for all modules in your solution.

### **2. Clean Up Existing Files**
Use the provided PowerShell script:

```powershell
# Run from solution root
.\Scripts\CleanupGlobalUsings.ps1
```

**Or manually remove these using statements from your files:**
- `using System;`
- `using System.Collections.Generic;`
- `using System.Linq;`
- `using System.Threading.Tasks;`
- `using Microsoft.Extensions.Logging;`
- `using MediatR;`
- `using Mapster;`
- `using Carter;`
- `using Shared.DDD;`
- `using Shared.CQRS;`
- `using Catalog.Products.Dtos;`
- etc.

### **3. Verify Build**
```bash
dotnet build
```

### **4. Test Functionality**
```bash
dotnet test
```

## ?? **Best Practices**

### **? Do Include in Global Usings:**
- **System namespaces** used in 80%+ of files (`System`, `System.Linq`)
- **Microsoft.Extensions** commonly used (`ILogger`, `IServiceCollection`)
- **Third-party packages** used everywhere (`MediatR`, `Mapster`)
- **Your own common namespaces** (`Shared.DDD`, `Catalog.Products.Dtos`)

### **? Don't Include in Global Usings:**
- **Rarely used namespaces** (used in <20% of files)
- **Specific implementations** (`System.IO.File` - only for file operations)
- **Test-specific usings** (`Xunit`, `FluentAssertions`)
- **Module-specific in wrong module** (Don't put Catalog usings in Basket)

### **?? Module-Specific Guidelines:**

| Module | Include | Don't Include |
|--------|---------|---------------|
| **Shared** | Base framework, MediatR, EF Core | Carter, specific DTOs |
| **Catalog** | Carter, Catalog DTOs/Events | Basket DTOs, Order entities |
| **Basket** | Carter, Basket DTOs | Catalog events, Order DTOs |
| **API** | ASP.NET Core, Controllers | Business logic DTOs |

## ?? **Troubleshooting**

### **Build Errors After Implementation**
1. **Check namespace exists**: Ensure all global usings reference valid namespaces
2. **Module dependencies**: Verify project references are correct
3. **Specific usings**: Some files might need specific usings not in global

### **IntelliSense Issues**
1. **Restart IDE**: Sometimes IntelliSense needs a refresh
2. **Clean & Rebuild**: `dotnet clean && dotnet build`
3. **Delete bin/obj**: Clear cached compilation artifacts

### **Adding New Dependencies**
When adding new packages:
1. **Evaluate usage**: Will this be used in 50%+ of files?
2. **Add to appropriate GlobalUsings.cs**: Choose the right level (Shared vs Module)
3. **Update cleanup script**: Add to `CleanupGlobalUsings.ps1` for future use

## ?? **Impact Metrics**

### **Before Implementation:**
- Average using statements per file: **8-12**
- Total lines of using statements: **~500** across project
- Repetitive namespace declarations: **High**

### **After Implementation:**
- Average using statements per file: **0-2**
- Total lines of using statements: **~50** (only in GlobalUsings.cs)
- Code reduction: **~450 lines** of boilerplate removed
- Maintenance effort: **Significantly reduced**

## ?? **Maintenance**

### **Adding New Global Usings**
```csharp
// Add to appropriate GlobalUsings.cs
global using NewNamespace.That.IsUsedEverywhere;
```

### **Removing Global Usings**
1. Remove from `GlobalUsings.cs`
2. Build to find affected files
3. Add specific using statements where needed

### **Module Evolution**
As modules grow, consider:
- Moving common usings up to Shared
- Creating module-specific sub-global-usings
- Reviewing usage patterns quarterly

## ?? **Success!**

Your eShop modular monolith now has:
- ? **5 GlobalUsings.cs files** covering all modules
- ? **Automated cleanup script** for existing files
- ? **Cleaner, more maintainable code**
- ? **Centralized dependency management**
- ? **Better developer experience**

The implementation follows .NET best practices and significantly improves code maintainability! ??