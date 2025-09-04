# eShop Modular Monolith - Docker Setup

This is a complete Docker Compose setup for the eShop Modular Monolith application built with .NET 8.

## ??? Architecture

The application consists of:
- **Catalog Module**: Product management and catalog functionality
- **Basket Module**: Shopping cart functionality  
- **Ordering Module**: Order processing and management
- **Shared Module**: Common DDD infrastructure and utilities

## ?? Services

### Core Services
- **eshop-api**: Main .NET 8 Web API application (Ports: 8080/8081)
- **eshop-db**: PostgreSQL 15 database (Port: 5432)
- **eshop-redis**: Redis cache for sessions/caching (Port: 6379)

### Management Tools
- **eshop-pgadmin**: Database administration tool (Port: 5050)
- **eshop-seq**: Structured logging and monitoring (Port: 5341)

## ?? Quick Start

### Prerequisites
- Docker Desktop
- .NET 8 SDK (for local development)

### Running the Application

1. **Clone and navigate to the project directory**
   ```bash
   cd /path/to/eshop-modular-monoliths/src
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **View logs**
   ```bash
   docker-compose logs -f eshop-api
   ```

4. **Stop services**
   ```bash
   docker-compose down
   ```

### Development Mode

For development with hot reload and debugging:

```bash
# Start infrastructure only
docker-compose up -d eshop-db eshop-redis eshop-seq eshop-pgadmin

# Run the API locally
dotnet run --project Bootstrapper/Api
```

Or use the development override:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

## ?? Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:8080 | - |
| API (HTTPS) | https://localhost:8081 | - |
| Swagger UI | http://localhost:8080/swagger | - |
| Health Checks | http://localhost:8080/health | - |
| pgAdmin | http://localhost:5050 | admin@eshop.com / admin123 |
| Seq Logs | http://localhost:5341 | - |

## ??? Database

### Connection Details
- **Host**: localhost (or eshop-db from containers)
- **Port**: 5432
- **Database**: eshopdb
- **Username**: eshopuser
- **Password**: eshoppass123

### Schemas
- `catalog`: Product and category data
- `basket`: Shopping cart data
- `ordering`: Order and payment data
- `shared`: Common/shared data

## ?? Configuration

### Environment Variables

Key environment variables that can be customized:

```env
# Database
POSTGRES_DB=eshopdb
POSTGRES_USER=eshopuser
POSTGRES_PASSWORD=eshoppass123

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080;https://+:8081

# Logging
Logging__LogLevel__Default=Information
```

### Connection Strings

The application uses these connection strings:

- **DefaultConnection**: Main database connection
- **CatalogConnection**: Catalog module database
- **BasketConnection**: Basket module database  
- **OrderingConnection**: Ordering module database
- **Redis**: Redis cache connection

## ?? Troubleshooting

### Common Issues

1. **Port conflicts**
   ```bash
   # Check what's using the ports
   netstat -an | findstr :8080
   netstat -an | findstr :5432
   ```

2. **Database connection issues**
   ```bash
   # Check database health
   docker-compose exec eshop-db pg_isready -U eshopuser -d eshopdb
   ```

3. **Application not starting**
   ```bash
   # Check application logs
   docker-compose logs eshop-api
   ```

4. **Clear all data and restart**
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

### Health Checks

The application provides several health check endpoints:

- `/health` - Overall application health
- `/health/ready` - Readiness probe (database connectivity)
- `/health/live` - Liveness probe (application responsiveness)

## ?? Monitoring and Logging

### Seq Structured Logging
- Access: http://localhost:5341
- All application logs are automatically sent to Seq
- Filter and search logs using structured queries
- Set up alerts and dashboards

### Database Monitoring via pgAdmin
- Access: http://localhost:5050
- Monitor database performance
- Run queries and manage data
- View connection statistics

## ??? Development

### Adding New Modules

1. Create new module project in `Modules/` directory
2. Add module registration in `Program.cs`
3. Update `docker-compose.yml` if additional services needed
4. Add module-specific database schema in init scripts

### Domain Events

The application uses MediatR for domain events:

```csharp
// Raising events
product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name));

// Handling events
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Handle the event
    }
}
```

## ?? Notes

- The application uses PostgreSQL with separate schemas for each module
- Redis is configured for caching and session storage
- All logs are structured and sent to Seq for analysis
- Health checks ensure proper startup ordering
- pgAdmin provides easy database management
- The application follows DDD principles with rich domain models

## ?? Data Persistence

All data is persisted using Docker volumes:
- `eshop_postgres_data`: Database data
- `eshop_redis_data`: Redis cache data  
- `eshop_pgadmin_data`: pgAdmin configuration
- `eshop_seq_data`: Seq logs and configuration

To completely reset all data:
```bash
docker-compose down -v
docker volume prune
```