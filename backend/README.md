# Weather App Backend

A comprehensive ASP.NET Core 9.0 Web API for weather tracking and forecasting, featuring automatic background synchronization, user-specific preferences, and real-time weather data from OpenWeatherMap.

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Background Job System](#background-job-system)
- [Entity Design](#entity-design)
- [Weather Data Synchronization](#weather-data-synchronization)
- [Troubleshooting](#troubleshooting)

---

## Features

- **User Authentication** - Secure registration and login with BCrypt password hashing
- **Location Management** - Add, track, and manage weather locations
- **Real-time Weather Data** - Integration with OpenWeatherMap API
- **Automatic Background Sync** - Scheduled weather data updates via Hangfire
- **User Preferences** - Customizable units (Metric/Imperial) and refresh intervals
- **5-Day Forecasts** - Daily and hourly weather predictions
- **Rate Limiting** - Manual refresh rate limiting to prevent API abuse
- **Global Sync** - Hourly synchronization for all locations
- **Job Tracking** - Monitor background job status per location

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Weather App Backend                       │
├─────────────────────────────────────────────────────────────────┤
│  Controllers                                                    │
│  ├── AuthController         - User registration & login         │
│  ├── LocationController     - Location CRUD operations          │
│  ├── TrackLocationController - Manage tracked locations         │
│  ├── UserPreferenceController - User settings & preferences     │
│  └── WeatherForecastController - Weather data endpoints         │
├─────────────────────────────────────────────────────────────────┤
│  Services (Business Logic)                                      │
│  ├── AuthService            - Authentication & JWT              │
│  ├── LocationService        - Location management + job enqueue │
│  ├── TrackLocationService   - User-location tracking            │
│  ├── UserPreferenceService  - User settings management          │
│  ├── WeatherForecastService - Weather data retrieval            │
│  ├── OpenWeatherService     - OpenWeatherMap API integration    │
│  ├── SyncScheduleService    - User-specific sync schedules      │
│  └── GlobalSyncService      - Global location sync              │
├─────────────────────────────────────────────────────────────────┤
│  Background Jobs (Hangfire)                                     │
│  ├── Global Sync Job        - Runs hourly for all locations     │
│  ├── User Sync Jobs         - Per-user scheduled syncs          │
│  └── Location Jobs          - On-demand weather fetch jobs      │
├─────────────────────────────────────────────────────────────────┤
│  Data Layer                                                     │
│  ├── AppDbContext           - EF Core DbContext                 │
│  ├── Entities               - Domain models                     │
│  └── DataTransferObjects    - API response/request models       │
├─────────────────────────────────────────────────────────────────┤
│  External Services                                              │
│  ├── SQL Server             - Primary database                  │
│  ├── Hangfire Dashboard     - Job monitoring UI                 │
│  └── OpenWeatherMap API     - Weather data provider             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

Ensure the following are installed on your system:

| Software | Version | Download Link |
|----------|---------|---------------|
| .NET SDK | 9.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| SQL Server | 2016+ or Express | [Download](https://www.microsoft.com/sql-server/sql-server-downloads) |
| IDE (Optional) | - | Visual Studio 2022, Rider, or VS Code |

### Verify Installation

```bash
# Check .NET SDK version
dotnet --version

# Check installed EF Core tools
dotnet ef --version
```

---

## Getting Started

### 1. Clone or Navigate to the Project

```bash
cd C:\Users\eugen\RiderProjects\weatherapp\backend
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Connection String

Edit `appsettings.json` with your SQL Server credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=weatherapp;User Id=admin;Password=admin$123;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  },
  "OpenWeatherApiKey": "your-api-key-here"
}
```

> **Note:** The current configuration uses a development API key. For production, obtain your own key from [OpenWeatherMap](https://openweathermap.org/api).

### 4. Create the Database

```bash
# Using EF Core migrations
dotnet ef database update
```

If migrations don't exist, create them first:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTPS:** `https://localhost:7001` (or configured port)
- **HTTP:** `http://localhost:5001` (or configured port)

### 6. Access Hangfire Dashboard

Navigate to: `https://localhost:7001/hangfire`

The dashboard provides real-time monitoring of all background jobs.

---

## Configuration

### appsettings.json

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | - |
| `OpenWeatherApiKey` | OpenWeatherMap API key | - |
| `Logging:LogLevel` | Logging verbosity levels | Information |

### Environment Variables (Optional)

For production deployments, use environment variables:

```bash
# Set connection string
setx ConnectionStrings__DefaultConnection "Server=...;Database=...;..."

# Set API key
setx OpenWeatherApiKey "your-api-key"
```

---

## Database Setup

### Entity Framework Core Commands

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply pending migrations
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list

# Drop the database (development only)
dotnet ef database drop --force
```

### Database Schema

The application creates the following tables:

| Table | Description |
|-------|-------------|
| `Users` | User accounts with hashed passwords |
| `Locations` | Weather locations (city, coordinates, country) |
| `TrackLocations` | User-location associations (favorites, display names) |
| `UserPreferences` | User settings (units, refresh intervals) |
| `DailyWeathers` | Daily weather forecasts (5 days) |
| `HourlyWeathers` | Hourly weather forecasts (24 hours) |
| `LocationJobs` | Background job tracking per location |
| `LocationSyncSchedules` | User-specific sync schedules with recurring job IDs |

---

## Running the Application

### Development Mode

```bash
dotnet run --environment Development
```

### Production Mode

```bash
dotnet run --environment Production
```

### Watch for Changes (Optional)

Install the watch tool:

```bash
dotnet tool install --global dotnet-watch
dotnet watch run
```

---

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive token |

### Locations

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/location` | Get all locations |
| POST | `/api/location` | Create a new location (triggers background job) |

### Track Locations

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tracklocation/user/{userId}` | Get all locations tracked by a user |
| POST | `/api/tracklocation/user/{userId}` | Start tracking a location |
| PUT | `/api/tracklocation/user/{userId}/{trackedLocationId}` | Update tracking settings |
| DELETE | `/api/tracklocation/user/{userId}/{trackedLocationId}` | Stop tracking a location |

### User Preferences

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/userpreference/user/{userId}` | Get user preferences |
| POST | `/api/userpreference/user/{userId}` | Create user preferences (initializes sync schedules) |
| PUT | `/api/userpreference/user/{userId}/{preferenceId}` | Update preferences (updates sync schedules) |
| DELETE | `/api/userpreference/user/{userId}/{preferenceId}` | Delete preferences (removes sync schedules) |

### Weather Forecasts

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/weatherforecast/current-day-summaries/{userId}` | Get current day summaries for all tracked locations |
| GET | `/api/weatherforecast/five-day/{locationId}` | Get 5-day forecast for a location |
| GET | `/api/weatherforecast/hourly/{locationId}` | Get 24-hour forecast for a location |

---

## Background Job System

The application uses **Hangfire** for robust background job processing with SQL Server storage.

### Job Types

#### 1. Global Sync Job
- **Schedule:** Every hour (`0 * * * *`)
- **Purpose:** Syncs weather data for ALL locations in the database
- **Use Case:** Ensures all locations have fresh data regardless of user activity
- **Job ID:** `global-locations-weather-sync`

#### 2. User-Specific Sync Jobs
- **Schedule:** Based on user preference (default: every 30 minutes)
- **Purpose:** Syncs weather data only for locations a specific user tracks
- **Use Case:** Efficient, personalized updates based on user settings
- **Job ID Format:** `sync:{userId}:{locationId}`

#### 3. On-Demand Location Jobs
- **Trigger:** When a new location is created
- **Purpose:** Immediately fetch weather data for new locations
- **Use Case:** Provides instant data availability without waiting for scheduled sync
- **Job Tracking:** Stored in `LocationJobs` table with status monitoring

### Job Flow: Adding a New Location

```
1. User creates location via POST /api/location
           │
           ▼
2. Location saved to database
           │
           ▼
3. Two background jobs enqueued:
   ├── GetLocationDailyWeather (5-day forecast)
   └── GetLocationHourlyWeather (24-hour forecast)
           │
           ▼
4. LocationJob entity created with status "Pending"
           │
           ▼
5. Hangfire processes jobs:
   ├── Fetches data from OpenWeatherMap API
   ├── Saves to DailyWeathers / HourlyWeathers tables
   └── Updates LocationJob status to "Completed"
           │
           ▼
6. API waits for job completion (max 30s) before responding
   (ensures data is available when response is sent)
```

### Job Status Tracking

The `LocationJobs` table tracks each job:

| Column | Description |
|--------|-------------|
| `LocationId` | Associated location |
| `JobId` | Hangfire job identifier |
| `JobCreatedAt` | When the job was enqueued |
| `Status` | Pending, Processing, Succeeded, Failed, Deleted |

### Sync Schedule Management

The `LocationSyncSchedules` table manages recurring jobs:

| Column | Description |
|--------|-------------|
| `UserId` | User who owns the schedule |
| `LocationId` | Location being synced |
| `LastSyncAt` | Timestamp of last successful sync |
| `NextSyncAt` | Scheduled time for next sync |
| `RecurringJobId` | Hangfire recurring job identifier |

### Rate Limiting

Manual refreshes are rate-limited to prevent API abuse:

- **Limit:** One manual refresh per 15 minutes per user
- **Enforcement:** `SyncScheduleService.RefreshWeatherForUserAsync()`
- **Response:** Returns `NextRefreshAllowedAt` timestamp

---

## Entity Design

### Core Entities

#### User
```
User
├── Id (Guid, PK)
├── Name (string, max 100)
├── Email (string, unique, max 255)
├── PasswordHash (string)
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime)
```

**Design Rationale:**
- Simple authentication model with BCrypt hashing
- No external identity provider dependency
- Audit timestamps for tracking changes

#### Location
```
Location
├── Id (Guid, PK)
├── Name (string, max 200)
├── Latitude (decimal(9,6))
├── Longitude (decimal(9,6))
├── Country (string, max 100)
├── DailyWeathers (ICollection<DayWeather>)
├── HourlyWeathers (ICollection<HourWeather>)
├── LocationJobs (ICollection<LocationJob>)
└── LocationSyncSchedules (ICollection<LocationSyncSchedule>)
```

**Design Rationale:**
- Central entity for all weather data
- Precise coordinate storage (6 decimal places = ~0.1m accuracy)
- Navigation properties for related weather data and jobs
- Locations are shared across users (not user-specific)

#### TrackLocation
```
TrackLocation
├── Id (Guid, PK)
├── UserId (Guid, FK → Users)
├── LocationId (Guid, FK → Locations)
├── IsFavorite (bool)
├── DisplayName (string, nullable, max 200)
├── User (Navigation)
└── Location (Navigation)
```

**Design Rationale:**
- Junction table connecting Users and Locations
- Allows users to track multiple locations
- `IsFavorite` for quick access to preferred locations
- `DisplayName` for user-customized location names
- **DeleteBehavior.NoAction** on User FK to prevent cascade delete issues

#### UserPreference
```
UserPreference
├── Id (Guid, PK)
├── UserId (Guid, FK → Users)
├── PreferredUnit (Unit enum: Metric/Imperial)
├── RefreshInterval (int, minutes)
├── LastManualRefreshAt (DateTime?, nullable)
└── User (Navigation)
```

**Design Rationale:**
- One-to-one relationship with User
- Stores user-specific settings
- `RefreshInterval` drives sync schedule frequency
- `LastManualRefreshAt` enables rate limiting

#### DayWeather
```
DayWeather
├── Id (Guid, PK)
├── LocationId (Guid, FK → Locations)
├── Date (DateOnly)
├── TimeOfForecast (DateTime)
├── MinTempMetric (decimal(5,2))
├── MaxTempMetric (decimal(5,2))
├── MinTempImperial (decimal(5,2))
├── MaxTempImperial (decimal(5,2))
├── Humidity (decimal(5,2))
├── Rain (decimal(6,2), nullable)
├── Summary (string, max 200)
└── Location (Navigation)
```

**Design Rationale:**
- Stores 5-day daily forecasts
- Both metric and imperial stored to avoid runtime conversion
- `DateOnly` for date-based queries
- Nullable `Rain` (not all forecasts include precipitation)

#### HourWeather
```
HourWeather
├── Id (Guid, PK)
├── LocationId (Guid, FK → Locations)
├── DateTime (DateTime)
├── TempMetric (decimal(5,2))
├── TempImperial (decimal(5,2))
├── Humidity (decimal(5,2))
└── Location (Navigation)
```

**Design Rationale:**
- Stores 24-hour hourly forecasts
- Lightweight entity with essential fields
- Precise timestamp for hourly data points

#### LocationJob
```
LocationJob
├── Id (Guid, PK)
├── LocationId (Guid, FK → Locations)
├── JobId (string, max 100)
├── JobCreatedAt (DateTime)
├── Status (string, max 50)
└── Location (Navigation)
```

**Design Rationale:**
- Tracks background job execution per location
- Enables API to wait for job completion before responding
- Provides visibility into data freshness

#### LocationSyncSchedule
```
LocationSyncSchedule
├── Id (Guid, PK)
├── UserId (Guid, FK → Users)
├── LocationId (Guid, FK → Locations)
├── LastSyncAt (DateTime)
├── NextSyncAt (DateTime)
├── RecurringJobId (string, max 200)
├── User (Navigation)
└── Location (Navigation)
```

**Design Rationale:**
- Manages user-specific recurring sync jobs
- Unique constraint on (UserId, LocationId) prevents duplicates
- Tracks sync history and scheduling
- Enables dynamic schedule updates when preferences change

---

## Weather Data Synchronization

### Synchronization Strategies

The application employs a **dual-layer synchronization** approach:

#### Layer 1: Global Sync (All Locations)
```csharp
// Runs every hour
Cron: "0 * * * *"
```

**Purpose:**
- Ensures ALL locations have baseline data freshness
- Independent of user activity
- Fallback for locations not actively tracked

**Implementation:**
```csharp
// GlobalSyncService.InitializeGlobalSyncAsync()
recurringJobManager.AddOrUpdate(
    "global-locations-weather-sync",
    () => openWeatherService.SyncLocationsDailyWeather(),
    "0 * * * *"
);
```

#### Layer 2: User-Specific Sync (Tracked Locations)
```csharp
// Runs based on user preference (default: every 30 minutes)
Cron: "*/30 * * * *" (for 30-minute interval)
```

**Purpose:**
- Efficient updates for actively tracked locations
- Respects user preferences (refresh interval)
- Reduces API calls for inactive users

**Implementation:**
```csharp
// SyncScheduleService.InitializeSyncSchedulesForUserAsync()
recurringJobManager.AddOrUpdate(
    $"sync:{userId}:{locationId}",
    () => openWeatherService.SyncWeatherForUserTrackedLocationsAsync(userId),
    cronExpression
);
```

### Sync Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    Weather Sync Architecture                     │
└─────────────────────────────────────────────────────────────────┘

┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│  Global Sync │         │  User Sync   │         │ On-Demand    │
│   (Hourly)   │         │ (Per-User)   │         │   Jobs       │
└──────┬───────┘         └──────┬───────┘         └──────┬───────┘
       │                        │                        │
       ▼                        ▼                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                    OpenWeatherService                            │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────────┐ │
│  │SyncLocationsDaily│  │SyncWeatherForUser│  │GetLocationDaily│ │
│  │Weather()         │  │TrackedLocations()│  │Weather()       │ │
│  └────────┬─────────┘  └────────┬─────────┘  └────────┬───────┘ │
└───────────┼─────────────────────┼─────────────────────┼─────────┘
            │                     │                     │
            ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                    OpenWeatherMap API                            │
│  https://api.openweathermap.org/data/3.0/onecall                 │
└─────────────────────────────────────────────────────────────────┘
            │                     │                     │
            ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Database Update                               │
│  ┌──────────────────┐  ┌──────────────────┐                     │
│  │  DailyWeathers   │  │  HourlyWeathers  │                     │
│  │  (5-day forecast)│  │  (24-hour)       │                     │
│  └──────────────────┘  └──────────────────┘                     │
└─────────────────────────────────────────────────────────────────┘
```

### Data Freshness Guarantees

| Scenario | Data Freshness |
|----------|----------------|
| Global Sync | Updated within 1 hour |
| User Sync | Updated per user interval (default: 30 min) |
| New Location | Immediate (on-demand job) |
| Manual Refresh | Immediate (rate-limited: 15 min) |

### Handling Duplicate Data

The sync process uses **upsert logic** to prevent duplicates:

```csharp
// Remove existing records for the same date/time
var existingHourly = context.HourlyWeathers
    .Where(w => w.LocationId == location.Id &&
                hourlyDateTimes.Contains(w.DateTime));
context.HourlyWeathers.RemoveRange(existingHourly);

// Add new records
await context.HourlyWeathers.AddRangeAsync(hourlyEntities);
```

This ensures:
- No duplicate timestamps per location
- Latest data always replaces old data
- Clean, consistent dataset

### Error Handling

All sync jobs include comprehensive error handling:

```csharp
try
{
    await GetLocationDailyWeather(location);
    await GetLocationHourlyWeather(location);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to sync weather for location {LocationName}", location.Name);
    // Job marked as Failed in Hangfire
    // Retry on next scheduled run
}
```

**Failure Scenarios:**
- API rate limits exceeded → Job fails, retries next cycle
- Network timeout → Job fails, retries next cycle
- Invalid location coordinates → Logged, job fails

---

## Troubleshooting

### Database Connection Issues

**Error:** `Cannot open database "weatherapp"`

**Solution:**
```bash
# Verify SQL Server is running
# Check connection string in appsettings.json
# Create database using migrations
dotnet ef database update
```

### Hangfire Jobs Not Running

**Error:** Jobs stuck in "Enqueued" or "Processing" state

**Solution:**
```bash
# Ensure Hangfire server is running (included in Program.cs)
# Check Hangfire dashboard: /hangfire
# Restart the application
# Verify SQL Server access for Hangfire tables
```

### API Key Issues

**Error:** OpenWeatherMap returns 401 Unauthorized

**Solution:**
1. Verify API key in `appsettings.json`
2. Check API key activation (new keys may take 10-60 minutes to activate)
3. Verify API call limits not exceeded

### Migration Errors

**Error:** `The database is not in sync with the model`

**Solution:**
```bash
# Drop and recreate database (development only)
dotnet ef database drop --force
dotnet ef database update

# Or update existing migration
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### CORS Issues (Frontend Integration)

**Error:** CORS policy blocks frontend requests

**Solution:**
- Ensure frontend runs on `http://localhost:3000` (configured in Program.cs)
- Update CORS policy if using different port:
```csharp
options.AddPolicy("AllowFrontendOrigin",
    policy => policy.WithOrigins("http://localhost:YOUR_PORT")
        .AllowAnyHeader()
        .AllowAnyMethod());
```

---

## Development Tools

### Hangfire Dashboard

Access at: `https://localhost:7001/hangfire`

**Features:**
- View all jobs (Succeeded, Failed, Processing, Enqueued)
- Monitor recurring job schedules
- Retry failed jobs manually
- View job history and execution details

### API Testing

Use the included `weatherapp.http` file or tools like:
- Postman
- Insomnia
- Swagger/OpenAPI (if enabled)

---

## Production Considerations

### Security

- **API Keys:** Store in environment variables or Azure Key Vault
- **Connection Strings:** Use managed identities or secure secret storage
- **Hangfire Dashboard:** Add authentication middleware
- **CORS:** Restrict to production frontend domain

### Scalability

- **Hangfire Storage:** Consider Redis for high-scale scenarios
- **Database:** Use connection pooling and read replicas
- **API Rate Limits:** Implement caching layer (Redis/MemoryCache)

### Monitoring

- **Logging:** Configure Application Insights or Serilog
- **Health Checks:** Add `/health` endpoint
- **Metrics:** Track job success rates and API response times

---

## License

This project is proprietary and confidential.

---

## Support

For issues or questions, contact the development team.
