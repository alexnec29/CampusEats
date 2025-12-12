.PHONY: help build test clean up down restart logs

# Default target
help:
	@echo "CampusEats Development Commands"
	@echo "================================"
	@echo "make build        - Build all Docker images"
	@echo "make test         - Run tests for API and client"
	@echo "make up           - Start all services"
	@echo "make down         - Stop all services"
	@echo "make restart      - Restart all services"
	@echo "make logs         - Show logs from all services"
	@echo "make clean        - Clean up Docker resources"
	@echo ""
	@echo "Development targets:"
	@echo "make build-api    - Build only the API"
	@echo "make build-client - Build only the client"
	@echo "make test-api     - Run only API tests"
	@echo "make test-client  - Run only client tests"

# Build all services
build:
	@echo "Building all services..."
	docker-compose build

# Build only the API
build-api:
	@echo "Building API..."
	docker-compose build api

# Build only the client
build-client:
	@echo "Building client..."
	docker-compose build client

# Run all tests
test: test-api test-client

# Test the API
test-api:
	@echo "Testing .NET API..."
	cd CampusEats.Test && dotnet test --verbosity normal

# Test the client
test-client:
	@echo "Testing React client..."
	cd campuseats.client && npm test -- --watchAll=false --passWithNoTests

# Start all services
up:
	@echo "Starting all services..."
	docker-compose up -d

# Stop all services
down:
	@echo "Stopping all services..."
	docker-compose down

# Restart all services
restart: down up

# Show logs
logs:
	docker-compose logs -f

# Clean up Docker resources
clean:
	@echo "Cleaning up Docker resources..."
	docker-compose down -v
	docker system prune -f

# Verify everything works (used by CI)
verify: build test
	@echo "✓ All builds and tests passed!"
