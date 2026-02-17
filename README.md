# Room Booking System - Backend API

**ASP.NET Core Web API for managing campus room reservations**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18+-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## About

A production-ready RESTful backend service designed to centralize and streamline campus room booking operations. This system eliminates manual, unstructured booking processes by providing structured management of buildings, rooms, and bookings with built-in conflict prevention and comprehensive status tracking.

### Why This Project?

**Problem**: Manual room booking processes lead to:
- ❌ Inconsistent booking records
- ❌ No centralized data management  
- ❌ Double bookings and conflicts
- ❌ No clear status tracking

**Solution**: This API provides:
- ✅ Single source of truth for all reservations
- ✅ Automatic conflict detection and prevention
- ✅ Clear booking lifecycle management (Pending → Approved/Rejected)
- ✅ Advanced filtering, search, and pagination
- ✅ Standardized RESTful API for easy integration

---

## Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/yogaprastyoo/2026-room-booking-backend.git
cd 2026-room-booking-backend

# 2. Set up environment
cp .env.example .env
# Edit .env with your PostgreSQL credentials

# 3. Run migrations
dotnet ef database update --project src/RoomBooking.Api.csproj

# 4. Start the application
dotnet run --project src/RoomBooking.Api.csproj

# 5. Access API documentation
# Open http://localhost:5255/scalar/v1 in your browser
```

> **Tip**: See Installation & Setup section below for detailed instructions.

---

## Table of Contents

- [About](#about)
- [Quick Start](#quick-start)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Project Structure](#project-structure)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Use Cases

### Target Users
- **Campus Administrators**: Manage room allocations and approve booking requests
- **Students & Staff**: Request and track room bookings for events and activities  
- **Frontend/Mobile Developers**: Integrate with the API to build user-facing applications

### Real-World Scenarios
- **Event Planning**: Book conference rooms for seminars and workshops
- **Study Groups**: Reserve study rooms for collaborative learning
- **Academic Activities**: Schedule classrooms for extra classes or tutoring
- **Administrative Meetings**: Manage meeting room reservations

### Additional Documentation
For detailed requirements and specifications:
- [Product Requirements Document (PRD)](docs/01_PRD.md)
- [Feature Specifications](docs/)
- [Database Schema](docs/05_Database-Spec.md)
- [API Contracts](docs/06_JSON-Spec.md)

---

## Features

### Core Functionality
- **Building Management**: Full CRUD operations for campus buildings
- **Room Management**: Manage rooms with capacity tracking and building associations
- **Booking Management**: Create, update, and soft-delete room reservations
- **Status Workflow**: Dedicated endpoint for booking status transitions
- **Conflict Detection**: Automatic validation to prevent overlapping bookings

### Data Features
- **Soft Deletion**: Bookings are soft-deleted for data retention
- **Pagination**: All list endpoints support pagination
- **Advanced Filtering**: Filter by building, room, status, date range, and borrower name
- **Standardized Responses**: Consistent API response envelope across all endpoints

---

## Architecture

The system follows a **layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│          Controllers Layer              │  ← HTTP Request/Response handling
│   (BookingController, RoomController)   │     Route mapping, validation
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│           Services Layer                │  ← Business logic
│   (BookingService, RoomService)         │     Validation, conflict detection
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Data Access Layer               │  ← Database operations
│   (ApplicationDbContext, EF Core)       │     Entity mappings, migrations
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│          PostgreSQL Database            │  ← Data persistence
└─────────────────────────────────────────┘
```

### Design Patterns
- **Dependency Injection**: Services registered and injected via ASP.NET Core DI container
- **Repository Pattern**: Implemented through EF Core DbContext
- **DTO Pattern**: Separation between domain entities and API contracts
- **Service Layer Pattern**: Business logic isolated from controllers

### Key Principles
- **No business logic in controllers**: Controllers delegate to services
- **Consistent response format**: All endpoints use `StandardApiResponse<T>` envelope
- **Snake_case naming**: JSON properties follow snake_case convention
- **UTC timestamps**: All datetime values stored and transmitted in UTC

---

## Tech Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Framework** | ASP.NET Core | 10.0 | Web API framework |
| **Language** | C# | 13.0 | Primary language |
| **Database** | PostgreSQL | 18+ | Relational database |
| **ORM** | Entity Framework Core | 10.0.3 | Database access and migrations |
| **DB Provider** | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | PostgreSQL provider for EF Core |
| **Naming Convention** | EFCore.NamingConventions | 10.0.1 | Automatic snake_case mapping |
| **Environment Config** | DotNetEnv | 3.1.1 | .env file support |
| **API Documentation** | Scalar.AspNetCore | 1.2.45 | Interactive API documentation UI |
| **OpenAPI** | Microsoft.AspNetCore.OpenApi | 10.0.3 | OpenAPI specification generation |

---

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

#### Required

| Tool | Version | Purpose | Download |
|------|---------|---------|----------|
| **.NET SDK** | 10.0+ | Runtime and build tools | [Download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **PostgreSQL** | 18+ | Database server | [Download](https://www.postgresql.org/download/) |

**Verify installations:**
```bash
# Check .NET version
dotnet --version  # Should show 10.0.x

# Check PostgreSQL
psql --version    # Should show 18.x or higher
```

#### Recommended
- **IDE**: [Visual Studio 2022](https://visualstudio.microsoft.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/) with C# extension
- **Git**: For version control
- **API Client**: Postman or Insomnia (optional, Scalar UI is included)

---

### Installation & Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/yogaprastyoo/2026-room-booking-backend.git
cd 2026-room-booking-backend
```

#### 2. Configure Database

Create a PostgreSQL database for the application:

```sql
-- Connect to PostgreSQL
psql -U postgres

-- Create database
CREATE DATABASE roombooking;

-- Verify
\l
```

#### 3. Configure Environment Variables

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` and update with your PostgreSQL connection details:

```env
DB_CONNECTION_STRING=Host=localhost;Database=roombooking;Username=postgres;Password=yourpassword;Port=5432
```

> [!IMPORTANT]
> **Security Best Practices**
> - `.env` is gitignored and should **NEVER** be committed to version control
> - Use `.env.example` as a template for required variables
> - For production, use actual environment variables or secure secret management
> - Never share credentials in public repositories

#### 4. Restore Dependencies

```bash
dotnet restore
```

#### 5. Apply Database Migrations

Run migrations to create database schema:

```bash
# From repository root
dotnet ef database update --project src/RoomBooking.Api.csproj

# OR from src directory
cd src
dotnet ef database update
```

#### 6. Build the Project

```bash
dotnet build
```

> **Note**: First-time setup complete! You're now ready to run the application.

---

## ⚙️ Environment Configuration

### Environment Variables

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `DB_CONNECTION_STRING` | Yes | PostgreSQL connection string | `Host=localhost;Database=roombooking;Username=postgres;Password=pass;Port=5432` |

### Alternative: Manual Environment Setup

If you prefer not to use `.env` file:

**Windows (PowerShell):**
```powershell
$env:DB_CONNECTION_STRING="Host=localhost;Database=roombooking;Username=postgres;Password=yourpassword;Port=5432"
```

**Linux/macOS:**
```bash
export DB_CONNECTION_STRING="Host=localhost;Database=roombooking;Username=postgres;Password=yourpassword;Port=5432"
```

### Environment Loading Behavior

The application loads environment variables in the following order:
1. `.env` file in repository root (if exists)
2. `.env` file in src directory (fallback)
3. System environment variables (production)

---

## Running the Application

### Development Mode (Recommended)

Run with hot reload for development:

```bash
# From repository root
dotnet watch run --project src/RoomBooking.Api.csproj

# OR from src directory
cd src
dotnet watch run
```

### Standard Run

```bash
# From repository root
dotnet run --project src/RoomBooking.Api.csproj

# OR from src directory
cd src
dotnet run
```

### Verify the Application is Running

The application will start on:
- **HTTP**: `http://localhost:5255`

Test the health endpoint:

```bash
curl http://localhost:5255/api/health
```

Expected response:
```json
{
  "success": true,
  "message": "API is running.",
  "data": {
    "status": "healthy",
    "timestamp": "2026-02-17T04:40:00.000Z"
  }
}
```

---

## API Documentation

### Interactive Documentation (Scalar UI)

When running in **Development** mode, access the interactive API documentation:

**URL**: `http://localhost:5255/scalar/v1`

The Scalar UI provides:
- Complete endpoint documentation
- Request/response schemas
- Try-it-out functionality
- Example requests and responses

### OpenAPI Specification

Raw OpenAPI spec available at: `http://localhost:5255/openapi/v1.json`

### API Endpoints Overview

| Resource | Endpoint | Methods |
|----------|----------|---------|
| **Health** | `/api/health` | GET |
| **Buildings** | `/api/buildings` | GET, POST |
| | `/api/buildings/{id}` | GET, PUT, DELETE |
| **Rooms** | `/api/rooms` | GET, POST |
| | `/api/rooms/{id}` | GET, PUT, DELETE |
| **Bookings** | `/api/bookings` | GET, POST |
| | `/api/bookings/{id}` | GET, PUT, DELETE |
| | `/api/bookings/{id}/status` | PATCH |

### Response Format

All API responses follow a standardized envelope:

**Success Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* response data */ }
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

### Naming Conventions
- **JSON fields**: `snake_case` (e.g., `booking_start`, `borrower_name`)
- **Timestamps**: ISO 8601 format in UTC (e.g., `2026-02-17T04:30:00.000Z`)
- **Enums**: PascalCase strings (e.g., `"Pending"`, `"Approved"`, `"Rejected"`)

---

## Database

### Entity Relationship

```
Building (1) ──────< (N) Room (1) ──────< (N) Booking
```

- One **Building** has many **Rooms**
- One **Room** has many **Bookings**
- **Bookings** support soft deletion (DeletedAt field)

### Entities

| Entity | Key Fields | Notes |
|--------|-----------|-------|
| **Building** | Id, Name, Code | Name and Code must be unique |
| **Room** | Id, BuildingId, Name, Capacity | (BuildingId + Name) must be unique |
| **Booking** | Id, RoomId, BorrowerName, BookingStart, BookingEnd, Status, DeletedAt | Soft delete via DeletedAt |

### Database Migrations

**Create a new migration:**
```bash
dotnet ef migrations add MigrationName --project src/RoomBooking.Api.csproj
```

**Apply migrations:**
```bash
dotnet ef database update --project src/RoomBooking.Api.csproj
```

**Rollback migration:**
```bash
dotnet ef database update PreviousMigrationName --project src/RoomBooking.Api.csproj
```

**Remove last migration (if not applied):**
```bash
dotnet ef migrations remove --project src/RoomBooking.Api.csproj
```

---

## Project Structure

```
2026-room-booking-backend/
├── src/                          # Source code
│   ├── Controllers/              # API controllers (HTTP layer)
│   │   ├── BookingController.cs  # Booking endpoints
│   │   ├── BuildingController.cs # Building endpoints
│   │   ├── RoomController.cs     # Room endpoints
│   │   └── HealthController.cs   # Health check endpoint
│   ├── Services/                 # Business logic layer
│   │   ├── Interfaces/           # Service contracts
│   │   ├── BookingService.cs     # Booking business logic
│   │   ├── BuildingService.cs    # Building business logic
│   │   └── RoomService.cs        # Room business logic
│   ├── Domain/                   # Domain entities
│   │   ├── Booking.cs            # Booking entity
│   │   ├── Building.cs           # Building entity
│   │   ├── Room.cs               # Room entity
│   │   └── BookingStatus.cs      # Status enum
│   ├── DTOs/                     # Data transfer objects
│   │   ├── Booking/              # Booking request/response DTOs
│   │   ├── Building/             # Building request/response DTOs
│   │   ├── Room/                 # Room request/response DTOs
│   │   └── Common/               # Shared DTOs (StandardApiResponse, PagedResult)
│   ├── Data/                     # Database context
│   │   └── ApplicationDbContext.cs
│   ├── Mappings/                 # Entity configurations
│   │   ├── BookingMapping.cs     # Booking entity configuration
│   │   ├── BuildingMapping.cs    # Building entity configuration
│   │   └── RoomMapping.cs        # Room entity configuration
│   ├── Middleware/               # Custom middleware
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Migrations/               # EF Core migrations
│   ├── Program.cs                # Application entry point
│   └── RoomBooking.Api.csproj    # Project file
├── docs/                         # Documentation
│   ├── 01_PRD.md                 # Product Requirements Document
│   ├── 02_Feature-Booking.md     # Booking feature spec
│   ├── 03_Feature-Status.md      # Status management spec
│   ├── 04_Feature-Filtering.md   # Filtering spec
│   ├── 05_Database-Spec.md       # Database specification
│   └── 06_JSON-Spec.md           # JSON contract specification
├── .env.example                  # Environment template
├── .env                          # Local environment (gitignored)
├── .gitignore                    # Git ignore rules
├── RoomBooking.sln               # Solution file
└── README.md                     # This file
```

---

## Development

### Common Commands

| Command | Description |
|---------|-------------|
| `dotnet build` | Build the solution |
| `dotnet run --project src/RoomBooking.Api.csproj` | Run from repository root |
| `cd src && dotnet run` | Run from src directory |
| `dotnet watch run --project src/RoomBooking.Api.csproj` | Run with hot reload |
| `dotnet test` | Run unit tests (when available) |
| `dotnet ef migrations add <Name> --project src/RoomBooking.Api.csproj` | Create new migration |
| `dotnet ef database update --project src/RoomBooking.Api.csproj` | Apply migrations |

### Code Conventions

- **Controllers**: No business logic, delegate to services
- **Services**: Contain all business logic and validation
- **DTOs**: Used for API contracts, separate from domain entities
- **Naming**: 
  - C# code: PascalCase for classes/methods, camelCase for variables
  - JSON: snake_case for all properties
  - Database: snake_case for tables and columns

### Development Workflow

1. **Create feature branch**: `git checkout -b feature/your-feature-name`
2. **Make changes**: Implement your feature
3. **Test locally**: Verify functionality
4. **Commit**: Use conventional commit messages
5. **Push**: `git push origin feature/your-feature-name`
6. **Create PR**: Submit for review

### Conventional Commit Format

```
<type>(<scope>): <description>

[optional body]
[optional footer]
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Examples**:
```
feat(booking): add conflict detection for overlapping bookings
fix(room): resolve capacity validation issue
docs(readme): update installation instructions
```

---

## Troubleshooting

### Database Connection Issues

**Problem**: `DB_CONNECTION_STRING environment variable is not set`

**Solution**:
- Verify `.env` file exists in repository root
- Check `.env` file contains `DB_CONNECTION_STRING`
- Ensure no extra spaces or quotes around the value
- Try setting environment variable manually (see [Environment Configuration](#-environment-configuration))

---

**Problem**: `Connection refused` or `could not connect to server`

**Solution**:
- Verify PostgreSQL service is running:
  ```bash
  # Windows
  Get-Service postgresql*
  
  # Linux/macOS
  sudo systemctl status postgresql
  ```
- Check connection string host, port, and credentials
- Ensure database exists: `psql -U postgres -l`

---

### Migration Issues

**Problem**: `No migrations configuration type was found`

**Solution**:
- Ensure you're running from repository root with `--project` flag
- Or navigate to `src/` directory first

---

**Problem**: `The database does not exist`

**Solution**:
```bash
# Create database manually
psql -U postgres -c "CREATE DATABASE roombooking;"

# Then apply migrations
dotnet ef database update --project src/RoomBooking.Api.csproj
```

---

### Port Conflicts

**Problem**: `Address already in use` or port 5001 is busy

**Solution**:
- Change port in `src/Properties/launchSettings.json`
- Or stop the conflicting process:
  ```bash
  # Windows
  netstat -ano | findstr :5001
  taskkill /PID <PID> /F
  
  # Linux/macOS
  lsof -ti:5001 | xargs kill -9
  ```

---

### Build Errors

**Problem**: `The type or namespace name could not be found`

**Solution**:
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

---

### Scalar UI Not Loading

**Problem**: Scalar documentation page not accessible

**Solution**:
- Ensure running in **Development** mode (Scalar only enabled in development)
- Check `ASPNETCORE_ENVIRONMENT` is set to `Development`
- Verify URL: `https://localhost:5001/scalar/v1`

---

## Contributing

We welcome contributions from the community! Whether it's bug fixes, new features, or documentation improvements, your help is appreciated.

### How to Contribute

1. **Fork the repository**
   ```bash
   # Click the "Fork" button on GitHub
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

3. **Make your changes**
   - Follow the [code conventions](#code-conventions)
   - Write clear, descriptive commit messages
   - Add tests if applicable

4. **Commit your changes**
   ```bash
   git commit -m "feat(scope): add new feature"
   # Use conventional commit format
   ```

5. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request**
   - Provide a clear description of your changes
   - Reference any related issues
   - Ensure all checks pass

### Commit Message Convention

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]
[optional footer]
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Examples**:
```
feat(booking): add conflict detection for overlapping bookings
fix(room): resolve capacity validation issue
docs(readme): update installation instructions
refactor(service): simplify booking validation logic
```

### Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help others learn and grow

---

## License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2026 Room Booking System Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

See [LICENSE](LICENSE) file for full details.

> **Note**: This project is developed as part of PBL (Project-Based Learning) coursework.

---

## Contact

### Project Information
- **Repository**: [github.com/yogaprastyoo/2026-room-booking-backend](https://github.com/yogaprastyoo/2026-room-booking-backend)
- **Issues**: [Report a bug or request a feature](https://github.com/yogaprastyoo/2026-room-booking-backend/issues)
- **Discussions**: [Join the conversation](https://github.com/yogaprastyoo/2026-room-booking-backend/discussions)

### Maintainer
- **GitHub**: [@yogaprastyoo](https://github.com/yogaprastyoo)

### Support
If you encounter any issues or have questions:
1. Check the [Troubleshooting](#-troubleshooting) section
2. Search [existing issues](https://github.com/yogaprastyoo/2026-room-booking-backend/issues)
3. Create a [new issue](https://github.com/yogaprastyoo/2026-room-booking-backend/issues/new) with detailed information

---

---

**Version**: 1.0.0 (MVP)  
**Last Updated**: February 2026
