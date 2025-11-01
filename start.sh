#!/bin/bash

# ============================================
# Quick Start Script - Linux/Mac
# Starts Multi-Tenant application with Docker
# ============================================

# Colors
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

echo -e "${CYAN}================================================${NC}"
echo -e "${CYAN}  Multi-Tenant Per Database - Quick Start${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

# Check Docker is running
echo -e "${YELLOW}Checking Docker...${NC}"
if ! docker version > /dev/null 2>&1; then
    echo -e "${RED}✗ Docker is not running. Please start Docker.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker is running${NC}"

# Create .env file if not exists
if [ ! -f ".env" ]; then
    echo -e "${YELLOW}Creating .env file from template...${NC}"
    cp .env.example .env
    echo -e "${GREEN}✓ .env file created${NC}"
else
    echo -e "${GREEN}✓ .env file already exists${NC}"
fi

# Stop and remove existing containers
echo ""
echo -e "${YELLOW}Cleaning up existing containers...${NC}"
docker-compose down -v 2>/dev/null
echo -e "${GREEN}✓ Cleanup complete${NC}"

# Build and start services
echo ""
echo -e "${YELLOW}Building and starting services...${NC}"
echo -e "${GRAY}This may take a few minutes on first run...${NC}"
docker-compose up -d --build

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Services started successfully${NC}"
    
    echo ""
    echo -e "${CYAN}================================================${NC}"
    echo -e "${CYAN}  Services are starting up...${NC}"
    echo -e "${CYAN}================================================${NC}"
    echo ""
    echo -e "${YELLOW}Waiting for services to be healthy...${NC}"
    
    # Wait for health checks
    sleep 30
    
    echo ""
    echo -e "${GREEN}================================================${NC}"
    echo -e "${GREEN}  🚀 Application is ready!${NC}"
    echo -e "${GREEN}================================================${NC}"
    echo ""
    echo -e "${CYAN}📍 Service URLs:${NC}"
    echo "   • API:        http://localhost:5231"
    echo "   • Swagger:    http://localhost:5231/swagger"
    echo "   • Health:     http://localhost:5231/health"
    echo "   • Seq Logs:   http://localhost:5341"
    echo "   • Adminer:    http://localhost:8080"
    echo ""
    echo -e "${CYAN}🔑 Default Credentials:${NC}"
    echo "   • SQL Server: sa / YourStrong@Passw0rd123"
    echo "   • Test User:  testuser / Test123"
    echo ""
    echo -e "${CYAN}📚 Quick Commands:${NC}"
    echo -e "${GRAY}   • View logs:     docker-compose logs -f${NC}"
    echo -e "${GRAY}   • Stop all:      docker-compose down${NC}"
    echo -e "${GRAY}   • Restart:       docker-compose restart${NC}"
    echo ""
    echo -e "${YELLOW}📖 Documentation: MultitenantPerDb/Docs/DOCKER_SETUP.md${NC}"
    echo ""
    
    # Open browser (if xdg-open available)
    if command -v xdg-open > /dev/null; then
        read -p "Open Swagger in browser? (Y/n): " openBrowser
        if [ "$openBrowser" != "n" ] && [ "$openBrowser" != "N" ]; then
            xdg-open http://localhost:5231/swagger
        fi
    fi
    
else
    echo -e "${RED}✗ Failed to start services${NC}"
    echo -e "${YELLOW}Check logs with: docker-compose logs${NC}"
    exit 1
fi
