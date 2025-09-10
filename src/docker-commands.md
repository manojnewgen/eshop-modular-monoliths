# eShop Docker Management Commands
# Copy this to your shell or save as docker-commands.md for reference

# ?? QUICK START COMMANDS
# =====================

# Start all services for development
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Start only infrastructure (for local API development)
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d eshop-db eshop-redis eshop-seq eshop-pgadmin messagebus

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f eshop-api
docker-compose logs -f eshop-db
docker-compose logs -f messagebus

# Rebuild and restart
docker-compose down && docker-compose up -d --build

# Clean everything (removes volumes - BE CAREFUL!)
docker-compose down -v --remove-orphans

# ?? TROUBLESHOOTING COMMANDS
# ===========================

# Check service status
docker-compose ps

# Check container health
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Clean up Docker system
docker system prune -f

# Rebuild specific service
docker-compose up -d --build eshop-api

# ?? SERVICE URLS (Development)
# =============================
# API: http://localhost:8080
# Database: localhost:5433 (dev port)
# pgAdmin: http://localhost:5050
# Seq Logs: http://localhost:5341
# RabbitMQ Management: http://localhost:15672
# Redis: localhost:6379