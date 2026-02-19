# Weather App - Docker Setup

Complete Docker setup for the Weather App, including backend API, frontend, and SQL Server database.

## Quick Start

### First Time Setup

```bash
# Build and start all services
docker compose up --build

# Or run in background (detached mode)
docker compose up -d --build
```

### Access Points

| Service      | URL                              | Credentials             |
| ------------ | -------------------------------- | ----------------------- |
| Frontend     | http://localhost:3000            | -                       |
| Backend API  | http://localhost:5000            | -                       |
| Health Check | http://localhost:5000/api/health | -                       |
| SQL Server   | localhost:1435                   | sa / Admin@Weather2026! |

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
docker compose logs -f backend
docker compose logs -f frontend
docker compose logs -f sqlserver

# Last 100 lines
docker compose logs --tail=100 backend
```

### View Running Containers

```bash
docker compose ps
```

### Rebuild

```bash
# Force rebuild without cache
docker compose build --no-cache
docker compose up -d

# Rebuild specific service
docker compose build backend
docker compose up -d backend
```

## Debugging

```bash
# Execute commands inside backend container
docker compose exec backend bash
docker compose exec backend dotnet --info

# Execute commands inside frontend container
docker compose exec frontend sh

# SQL Server connection
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Admin@Weather2026!' -C

# View container stats
docker stats

# Inspect container
docker inspect weatherapp-api
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker compose logs backend

# Check service status
docker compose ps

# Verify SQL Server is healthy first
docker compose logs sqlserver
```

## Environment Variables

| Variable            | Default              | Description               |
| ------------------- | -------------------- | ------------------------- |
| `MSSQL_SA_PASSWORD` | `Admin@Weather2026!` | SQL Server admin password |
| `Database`          | `weatherapp`         | Database name             |
| `OpenWeatherApiKey` | From config          | OpenWeatherMap API key    |

> **Security Note:** Change default passwords in production!

## Volume Management

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect weatherapp_sqlserver-data

# Remove specific volume
docker volume rm weatherapp_sqlserver-data
```

## Network

```bash
# List networks
docker network ls

# Inspect network
docker network inspect weatherapp_weatherapp-network
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Compose Stack                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐ │
│  │   Frontend   │────▶│   Backend    │────▶│  SQL Server  │ │
│  │  (Next.js)   │     │  (ASP.NET 9) │     │  (2022-LTS)  │ │
│  │  Port 3000   │     │  Port 5000   │     │  Port 1435   │ │
│  └──────────────┘     └──────────────┘     └──────────────┘ │
│                                                              │
│  Network: weatherapp-network (bridge)                        │
│  Volumes: sqlserver-data (persistent storage)                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Services

### SQL Server

- Image: `mcr.microsoft.com/mssql/server:2022-latest`
- Persistent volume for data storage
- Health check enabled

### Backend

- .NET 9 ASP.NET Core Web API
- Depends on SQL Server (waits for health check)
- Hangfire for background jobs

### Frontend

- Next.js 16 application
- Depends on Backend
- Health check enabled
