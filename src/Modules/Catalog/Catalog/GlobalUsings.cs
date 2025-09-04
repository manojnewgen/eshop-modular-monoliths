// Global usings for the Catalog module
// Common .NET namespaces
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

// ASP.NET Core
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;

// Entity Framework
global using Microsoft.EntityFrameworkCore;

// Third-party packages
global using MediatR;
global using Mapster;
global using Carter;

// Shared module namespaces (inherited from Shared project reference)
global using Shared.DDD;
global using Shared.CQRS;
global using Shared.Mapping;

// Catalog module namespaces
global using Catalog.Data;
global using Catalog.Products.Dtos;
global using Catalog.Products.Events;global using Catalog.Products.Events;