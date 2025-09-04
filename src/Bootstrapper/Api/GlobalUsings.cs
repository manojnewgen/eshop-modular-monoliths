global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;

// ASP.NET Core
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;

// Entity Framework
global using Microsoft.EntityFrameworkCore;

// Third-party packages
global using MediatR;
global using Mapster;
global using Carter;
global using FluentValidation;

// Shared module namespaces
global using Shared.DDD;
global using Shared.CQRS;
global using Shared.Mapping;
global using Shared.Extentions;
global using Shared.Exceptions;
global using Shared.Exceptions.Extensions;
global using Catalog;
global using Basket;
global using Odering;
global using Shared.Data.Extensions;
global using Shared.Extentions;
global using Carter;