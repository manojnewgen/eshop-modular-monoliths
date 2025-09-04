// Global usings for the Shared module
// Common .NET namespaces
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

// ASP.NET Core
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;

// Entity Framework
global using Microsoft.EntityFrameworkCore;

// Third-party packages
global using MediatR;
global using Mapster;
global using FluentValidation;

// Shared module namespaces
global using Shared.DDD;
global using Shared.CQRS;
global using Shared.Mapping;
global using Shared.Exceptions;
global using Shared.Behaviors;