#!/bin/bash

# eShop Docker Compose Management Script

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_usage() {
    echo -e "${BLUE}eShop Docker Compose Management${NC}"
    echo ""
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  up          Start all services"
    echo "  down        Stop all services"
    echo "  restart     Restart all services"
    echo "  logs        Show logs for all services"
    echo "  logs-api    Show logs for API only"
    echo "  logs-db     Show logs for database only"
    echo "  status      Show status of all services"
    echo "  clean       Stop services and remove volumes"
    echo "  build       Build and start services"
    echo "  dev         Start development environment (infrastructure only)"
    echo "  help        Show this help message"
}

start_services() {
    echo -e "${GREEN}Starting eShop services...${NC}"
    docker-compose up -d
    echo -e "${GREEN}Services started successfully!${NC}"
    show_access_info
}

stop_services() {
    echo -e "${YELLOW}Stopping eShop services...${NC}"
    docker-compose down
    echo -e "${GREEN}Services stopped successfully!${NC}"
}

restart_services() {
    echo -e "${YELLOW}Restarting eShop services...${NC}"
    docker-compose restart
    echo -e "${GREEN}Services restarted successfully!${NC}"
}

show_logs() {
    echo -e "${BLUE}Showing logs for all services...${NC}"
    docker-compose logs -f
}

show_api_logs() {
    echo -e "${BLUE}Showing logs for API service...${NC}"
    docker-compose logs -f eshop-api
}

show_db_logs() {
    echo -e "${BLUE}Showing logs for database service...${NC}"
    docker-compose logs -f eshop-db
}

show_status() {
    echo -e "${BLUE}Service Status:${NC}"
    docker-compose ps
}

clean_environment() {
    echo -e "${RED}Cleaning environment (this will remove all data!)${NC}"
    read -p "Are you sure? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose down -v
        docker-compose down --rmi all
        echo -e "${GREEN}Environment cleaned successfully!${NC}"
    else
        echo -e "${YELLOW}Operation cancelled.${NC}"
    fi
}

build_and_start() {
    echo -e "${GREEN}Building and starting eShop services...${NC}"
    docker-compose up -d --build
    echo -e "${GREEN}Services built and started successfully!${NC}"
    show_access_info
}

start_dev_environment() {
    echo -e "${GREEN}Starting development environment (infrastructure only)...${NC}"
    docker-compose up -d eshop-db eshop-redis eshop-seq eshop-pgadmin
    echo -e "${GREEN}Development infrastructure started!${NC}"
    echo -e "${YELLOW}You can now run the API locally with: dotnet run --project Bootstrapper/Api${NC}"
    show_access_info
}

show_access_info() {
    echo -e "${BLUE}"
    echo "========================================"
    echo "?? eShop Services Access Information"
    echo "========================================"
    echo -e "${NC}"
    echo "?? API Application:"
    echo "   HTTP:  http://localhost:8080"
    echo "   HTTPS: https://localhost:8081"
    echo "   Health: http://localhost:8080/health"
    echo ""
    echo "???  Database (PostgreSQL):"
    echo "   Host: localhost:5432"
    echo "   Database: eshopdb"
    echo "   Username: eshopuser"
    echo "   Password: eshoppass123"
    echo ""
    echo "?? pgAdmin (Database Management):"
    echo "   URL: http://localhost:5050"
    echo "   Email: admin@eshop.com"
    echo "   Password: admin123"
    echo ""
    echo "?? Seq (Logging):"
    echo "   URL: http://localhost:5341"
    echo ""
    echo "???  Redis Cache:"
    echo "   Host: localhost:6379"
    echo ""
}

# Main script logic
case ${1:-help} in
    up)
        start_services
        ;;
    down)
        stop_services
        ;;
    restart)
        restart_services
        ;;
    logs)
        show_logs
        ;;
    logs-api)
        show_api_logs
        ;;
    logs-db)
        show_db_logs
        ;;
    status)
        show_status
        ;;
    clean)
        clean_environment
        ;;
    build)
        build_and_start
        ;;
    dev)
        start_dev_environment
        ;;
    help|*)
        print_usage
        ;;
esac