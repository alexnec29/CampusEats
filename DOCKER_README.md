# CampusEats Docker Setup

This project includes Docker support for easy development and deployment.

## Prerequisites

- Docker
- Docker Compose
- Make (optional, for convenient commands)

## Quick Start

### Using Docker Compose

1. **Start all services:**
   ```bash
   docker-compose up -d
   ```

2. **View logs:**
   ```bash
   docker-compose logs -f
   ```

3. **Stop all services:**
   ```bash
   docker-compose down
   ```

### Using Make

The project includes a Makefile with convenient commands:

```bash
make help       # Show all available commands
make build      # Build all Docker images
make test       # Run tests for API and client
make up         # Start all services
make down       # Stop all services
make restart    # Restart all services
make logs       # Show logs from all services
make clean      # Clean up Docker resources
```

## Configuration

### Environment Variables

Copy `.env.example` to `.env` and customize as needed:

```bash
cp .env.example .env
```

Available variables:
- `POSTGRES_DB` - PostgreSQL database name (default: campuseats_db)
- `POSTGRES_USER` - PostgreSQL username (default: postgres)
- `POSTGRES_PASSWORD` - PostgreSQL password (default: password)

**Important:** Change the default password in production!

## Services

The docker-compose setup includes:

1. **PostgreSQL** (Port 5432)
   - Database for the API
   - Data persisted in Docker volume

2. **API** (Port 5078)
   - .NET 9.0 API
   - Automatically runs migrations on startup
   - Health check endpoint at `/health`

3. **Client** (Port 3000)
   - React application
   - Served with nginx

## Health Checks

All services include health checks:
- PostgreSQL: `pg_isready`
- API: HTTP check at `/health`
- Client: HTTP check at root

## Development

### Building Individual Services

```bash
# Build only the API
make build-api

# Build only the client
make build-client
```

### Running Tests

```bash
# Run all tests
make test

# Run only API tests
make test-api

# Run only client tests
make test-client
```

## CI/CD

The project includes GitHub Actions workflow that automatically:
- Builds the .NET API
- Runs all .NET tests
- Builds the React client
- Runs client tests
- Verifies Docker images build successfully

This workflow runs on every pull request to any branch.

## Troubleshooting

### Services won't start
- Check if ports 5432, 5078, or 3000 are already in use
- View logs with `docker-compose logs`

### Database connection issues
- Wait for PostgreSQL to be ready (health check)
- Check connection string in docker-compose.yml

### Permission issues
- Ensure Docker daemon is running
- Check your user has Docker permissions

## Cleaning Up

```bash
# Remove all containers and volumes
make clean

# Or manually
docker-compose down -v
docker system prune -f
```
