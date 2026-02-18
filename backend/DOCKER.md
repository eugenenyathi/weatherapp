# Docker Quick Reference

## Quick Start

```bash
# First time setup - Build and run
docker compose up --build

# Run in background (detached mode)
docker compose up -d --build

# View running containers
docker compose ps

# View logs
docker compose logs -f
docker compose logs -f weatherapp
docker compose logs -f sqlserver
```

## Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| API (HTTP) | http://localhost:5000 | - |
| API (HTTPS) | https://localhost:5001 | - |
| Health Check | http://localhost:5000/api/health | - |
| Hangfire Dashboard | http://localhost:5000/hangfire | - |
| SQL Server | localhost:1433 | sa / YourStrong@Pass123 |

## Common Commands

### Start/Stop
```bash
# Start all services
docker compose up -d

# Stop all services
docker compose down

# Stop and remove volumes (DELETES ALL DATA)
docker compose down -v
```

### View Logs
```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f weatherapp
docker compose logs -f sqlserver

# Last 100 lines
docker compose logs --tail=100 weatherapp
```

### Database Migrations
```bash
# Run migrations inside container
docker compose exec weatherapp dotnet ef database update

# Create new migration
docker compose exec weatherapp dotnet ef migrations add MigrationName
```

### Debugging
```bash
# Execute commands inside container
docker compose exec weatherapp bash
docker compose exec weatherapp dotnet --info

# SQL Server connection
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Pass123'

# View container stats
docker stats

# Inspect container
docker inspect weatherapp-api
```

### Rebuild
```bash
# Force rebuild without cache
docker compose build --no-cache
docker compose up -d

# Rebuild specific service
docker compose build weatherapp
docker compose up -d weatherapp
```

### Clean Up
```bash
# Remove stopped containers
docker compose down

# Remove volumes (deletes data)
docker compose down -v

# Remove orphaned containers
docker compose down --remove-orphans

# Prune unused resources
docker system prune -a
```

## Development Mode

```bash
# Run with development settings
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build

# Access with development configuration
# API: http://localhost:5000
# Hangfire: http://localhost:5000/hangfire
```

## Troubleshooting

### Container Won't Start
```bash
# Check logs
docker compose logs weatherapp

# Check service status
docker compose ps

# Verify SQL Server is healthy first
docker compose logs sqlserver
```

### Database Connection Issues
```bash
# Ensure using correct service name in connection string
# Server=sqlserver (not localhost)

# Wait for SQL Server health check
docker compose ps
# Wait until sqlserver shows "healthy"
```

### Port Already in Use
```bash
# Check what's using the port
netstat -ano | findstr :5000
netstat -ano | findstr :5001
netstat -ano | findstr :1433

# Stop conflicting services or change ports in docker-compose.yml
```

### Reset Everything
```bash
# Complete reset
docker compose down -v
docker compose build --no-cache
docker compose up --build
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `MSSQL_SA_PASSWORD` | `YourStrong@Pass123` | SQL Server admin password |
| `Database` | `weatherapp` | Database name |
| `OpenWeatherApiKey` | From appsettings | OpenWeatherMap API key |

> **Security Note:** Change default passwords in production!

## Volume Management

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect backend_sqlserver-data

# Remove specific volume
docker volume rm backend_sqlserver-data
```

## Network

```bash
# List networks
docker network ls

# Inspect network
docker network inspect backend_weatherapp-network
```
