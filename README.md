# Room Booking System - Backend API

ASP.NET Core Web API for managing campus room reservations.

## Prerequisites

- .NET 10 SDK
- PostgreSQL database

## Quick Start

### 1. Configure Environment Variables

Copy the example environment file:
```bash
cp .env.example .env
```

Edit `.env` and update with your PostgreSQL connection details:
```env
DB_CONNECTION_STRING=Host=localhost;Database=roombooking;Username=postgres;Password=yourpassword;Port=5432
```

### 2. Run the Application

From repository root:
```bash
dotnet run --project src/RoomBooking.Api.csproj
```

Or from src directory:
```bash
cd src
dotnet run
```

### 3. Verify Setup

Test the health endpoint:
```bash
curl https://localhost:5001/api/health
```

Expected response:
```json
{
  "success": true,
  "message": "API is running.",
  "data": {
    "status": "healthy",
    "timestamp": "2026-02-14T16:40:00.000Z"
  }
}
```

## Environment Configuration

The application loads environment variables from `.env` file automatically.

**Important:** 
- `.env` is gitignored and should NOT be committed
- Use `.env.example` as a template
- For production, use actual environment variables instead of .env file

### Manual Environment Variable Setup (Alternative)

**Windows (PowerShell):**
```powershell
$env:DB_CONNECTION_STRING="Host=localhost;Database=roombooking;Username=postgres;Password=yourpassword"
```

**Linux/macOS:**
```bash
export DB_CONNECTION_STRING="Host=localhost;Database=roombooking;Username=postgres;Password=yourpassword"
```

## Development Commands

| Command | Description |
|---------|-------------|
| `dotnet build` | Build the solution |
| `dotnet run --project src/RoomBooking.Api.csproj` | Run from root |
| `cd src && dotnet run` | Run from src directory |
| `dotnet watch run --project src/RoomBooking.Api.csproj` | Run with hot reload |

## Project Structure

```
2026-room-booking-backend/
├── src/             # Source code
│   ├── Controllers/ # API controllers
│   ├── Services/    # Business logic layer
│   ├── Domain/      # Domain entities
│   ├── DTOs/        # Data transfer objects
│   ├── Data/        # DbContext and database configuration
│   ├── Mappings/    # Entity mappings
│   └── Middleware/  # Custom middleware
├── docs/            # Documentation
├── .env.example     # Environment template
├── .env             # Local environment (gitignored)
├── RoomBooking.sln  # Solution file
└── README.md        # This file
```

## Development Notes

- All API responses follow a standardized envelope format
- JSON fields use snake_case naming convention
- All timestamps are stored as PostgreSQL `timestamptz` (UTC)
- Global exception handling is configured
- No business logic in controllers