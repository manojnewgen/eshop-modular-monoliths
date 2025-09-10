# eShop Docker Management Commands
# Copy this to your shell or save as docker-commands.md for reference

# ?? QUICK START COMMANDS (LOCAL POSTGRESQL)
# ===========================================

# Start with containerized PostgreSQL
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Start with LOCAL PostgreSQL (recommended for development)
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d

# Start only infrastructure for local development (no database container)
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d eshop-redis eshop-seq messagebus identity

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f eshop-api
docker-compose logs -f identity
docker-compose logs -f messagebus

# Rebuild and restart
docker-compose down && docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d --build

# Clean everything (removes volumes - BE CAREFUL!)
docker-compose down -v --remove-orphans

# ??? LOCAL POSTGRESQL SETUP WITH SCHEMAS
# =======================================

# 1. Create database and user in your local PostgreSQL:
# Connect as superuser:
# psql -U postgres

# Create database and user:
# CREATE DATABASE eshopdb;
# CREATE USER eshopuser WITH ENCRYPTED PASSWORD 'EShop123!';
# GRANT ALL PRIVILEGES ON DATABASE eshopdb TO eshopuser;
# \q

# 2. Run the schema setup script:
# psql -U postgres -d eshopdb -f database-schema-setup.sql
# 
# OR run the simple version step by step:
# psql -U postgres -d eshopdb -f simple-schema-setup.sql

# 3. Test connection with schemas:
# psql -h localhost -p 5432 -U eshopuser -d eshopdb -c "\dn"

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

# Restart identity service
docker-compose restart identity

# Check database schemas
# psql -U eshopuser -d eshopdb -c "SELECT schema_name FROM information_schema.schemata WHERE schema_name IN ('catalog', 'basket', 'ordering', 'identity', 'shared', 'messaging') ORDER BY schema_name;"

# ?? SERVICE URLS (Local PostgreSQL Setup)
# ========================================
# API: http://localhost:8080
# Local PostgreSQL: localhost:5432
# Seq Logs: http://localhost:5341
# RabbitMQ Management: http://localhost:15672
# Redis: localhost:6379
# Keycloak Admin: http://localhost:9090/admin
# Keycloak Realm: http://localhost:9090/realms/eshop-dev

# ?? KEYCLOAK DEVELOPMENT CREDENTIALS
# ===================================
# Admin Console: admin / admin123
# Test User: testuser / test123
# Test Admin: admin / admin123

# ??? DATABASE CONNECTION INFO
# ============================
# Host: localhost (for local tools)
# Host: host.docker.internal (for containers to connect to local PostgreSQL)
# Port: 5432
# Database: eshopdb
# Username: eshopuser
# Password: EShop123!

# ?? SCHEMA INFORMATION
# ====================
# Available schemas:
# - catalog: Product catalog tables (products, categories)
# - basket: Shopping cart tables (shopping_carts, shopping_cart_items)
# - ordering: Order processing tables (orders, order_items)
# - identity: Keycloak identity tables (auto-created by Keycloak)
# - shared: Shared/audit tables (audit_logs, domain_events)
# - messaging: Event/message tables (integration_events, outbox_events)

# ?? API TESTING WITH KEYCLOAK
# ============================
# Get token for test user:
# curl -X POST http://localhost:9090/realms/eshop-dev/protocol/openid-connect/token \
#   -H "Content-Type: application/x-www-form-urlencoded" \
#   -d "grant_type=password&client_id=eshop-api-dev&client_secret=eshop-api-secret&username=testuser&password=test123"

# Use token in API calls:
# curl -H "Authorization: Bearer <token>" http://localhost:8080/api/products

# ?? DATABASE VERIFICATION COMMANDS
# =================================
# Connect to database:
# psql -U eshopuser -d eshopdb

# List all schemas:
# \dn

# List tables in a specific schema:
# \dt catalog.*
# \dt basket.*
# \dt ordering.*

# Switch search path to a schema:
# SET search_path TO catalog, shared, public;
# \dt