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
- ✅ Automatic conflict detection (Only **Approved** bookings block slots)
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
- **Status Workflow**: Dedicated endpoint for booking status transitions (Pending → Approved/Rejected)
- **Conflict Detection**: 
    - Prevents overlapping bookings.
    - **Logic**: Only `Approved` bookings block a time slot. `Pending` and `Rejected` bookings do not.
    - Soft-deleted bookings are ignored.

### Data Features
- **Soft Deletion**: Bookings are soft-deleted for data retention
- **Pagination**: 
    - Supported on all list endpoints.
    - Limits: Max **50** for Bookings, Max **100** for Buildings/Rooms.
- **Advanced Filtering**: Filter by building, room, status, date range, and borrower name
- **Standardized Responses**: Consistent API response envelope (`StandardApiResponse`) across all endpoints

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
  "data": { /* response data */ },
  "errors": null
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": { "field": ["error"] } 
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
| **Booking** | Id, RoomId, BorrowerName, BookingStart, BookingEnd, Status, DeletedAt | Soft delete via DeletedAt, Check Constraints Enforced |

---

## Project Structure

```
2026-room-booking-backend/
├── src/                          # Source code
│   ├── Controllers/              # API controllers (HTTP layer)
│   ├── Services/                 # Business logic layer
│   ├── Domain/                   # Domain entities
│   ├── DTOs/                     # Data transfer objects
│   ├── Data/                     # Database context
│   ├── Mappings/                 # Entity configurations
│   ├── Middleware/               # Custom middleware
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
├── README.md                     # This file
```

---

## Development

### Common Commands

| Command | Description |
|---------|-------------|
| `dotnet build` | Build the solution |
| `dotnet run --project src/RoomBooking.Api.csproj` | Run from repository root |
| `dotnet watch run --project src/RoomBooking.Api.csproj` | Run with hot reload |
| `dotnet ef database update --project src/RoomBooking.Api.csproj` | Apply migrations |

### Code Conventions

- **Controllers**: No business logic, delegate to services
- **Services**: Contain all business logic and validation
- **DTOs**: Used for API contracts, separate from domain entities
- **Naming**: 
  - C# code: PascalCase for classes/methods, camelCase for variables
  - JSON: snake_case for all properties
  - Database: snake_case for tables and columns

---

## License

This project is licensed under the **MIT License**.

See [LICENSE](LICENSE) file for full details.

---

**Version**: 1.0.0 (MVP)  
**Last Updated**: February 2026
