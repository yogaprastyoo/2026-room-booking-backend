# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-17

### Added
- Initial MVP release for Project-Based Learning (PBL) completion
- **Building Management**: Full CRUD operations for campus buildings
- **Room Management**: Manage rooms with capacity tracking and building associations
- **Booking Management**: 
  - Create, view, update, and soft-delete bookings
  - Status workflow: Pending → Approved/Rejected
  - Conflict detection to prevent overlapping reservations
- **Advanced Querying**: Filtering, search, and pagination for all list endpoints
- **API Documentation**: Interactive documentation via Scalar UI (`/scalar/v1`) and OpenAPI spec
- **Database**: PostgreSQL with Entity Framework Core migrations and snake_case naming convention
- **Health Check**: Endpoint to verify API status (`/api/health`)
- **Documentation**: Comprehensive README.md with setup instructions and architecture overview

### Technical Details
- Framework: ASP.NET Core 10.0
- Language: C# 13.0
- Database: PostgreSQL 18+
- ORM: EF Core 10.0.3
- Documentation: Scalar.AspNetCore 1.2.45
