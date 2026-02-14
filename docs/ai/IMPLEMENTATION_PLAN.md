# Implementation Plan – Room Booking Backend (PBL 2026)

This document defines the execution order and development strategy for the Room Booking System Backend.

All development must follow AI_RULES.md and PRD constraints.

---

# 1. Development Strategy

The implementation will follow an incremental feature-based approach:

1. Establish technical foundation
2. Implement Building module
3. Implement Room module
4. Implement Booking core
5. Implement Status update
6. Implement Filtering & Pagination
7. Finalize documentation and release

Each feature must:
- Be developed in a separate feature branch
- Be linked to a GitHub Issue
- Be merged via Pull Request
- Follow Conventional Commit format

---

# 2. Phase 1 – Project Foundation

## Goal
Prepare technical base before implementing business logic.

## Tasks

- Create ASP.NET Core Web API project (.NET 10)
- Configure folder structure:
  - Controllers/
  - Services/
  - Domain/
  - DTOs/
  - Data/
  - Mappings/
- Install required packages:
  - Npgsql.EntityFrameworkCore.PostgreSQL
  - Microsoft.EntityFrameworkCore.Design
- Configure DbContext
- Configure PostgreSQL connection via environment variable
- Configure snake_case JSON serialization
- Implement global response envelope
- Implement basic exception handling middleware

## Completion Criteria

- Project builds successfully
- API runs
- Database connection works
- Initial migration created

---

# 3. Phase 2 – Building Module

## Goal
Implement and stabilize Building CRUD before moving to dependent entities.

## Implementation Steps

1. Implement Building entity (Domain layer)
2. Configure Fluent API:
   - Unique Name
   - Unique Code
   - Max length validation
3. Create migration
4. Implement DTOs
5. Implement Service layer logic
6. Implement Controller endpoints
7. Implement pagination

## Endpoints

- POST /api/buildings
- GET /api/buildings
- GET /api/buildings/{id}
- PUT /api/buildings/{id}
- DELETE /api/buildings/{id}

## Completion Criteria

- All endpoints functional
- Pagination works
- Cannot delete building if rooms exist
- Response envelope consistent

---

# 4. Phase 3 – Room Module

## Goal
Implement Room with proper relational integrity.

## Implementation Steps

1. Implement Room entity
2. Configure:
   - FK to Building
   - UNIQUE(building_id, name)
   - Positive capacity validation
3. Create migration
4. Implement DTOs
5. Implement Service logic
6. Implement Controller endpoints
7. Add filtering by building_id and name

## Endpoints

- POST /api/rooms
- GET /api/rooms
- GET /api/rooms/{id}
- PUT /api/rooms/{id}
- DELETE /api/rooms/{id}

## Completion Criteria

- Composite unique constraint enforced
- Filtering works
- Cannot delete room if active bookings exist
- Pagination works

---

# 5. Phase 4 – Booking Core

## Goal
Implement booking logic including soft delete and conflict prevention.

## Implementation Steps

1. Implement Booking entity
2. Add:
   - FK to Room
   - DeletedAt field
   - CHECK (booking_end > booking_start)
   - Global query filter for soft delete
3. Create migration
4. Implement DTOs
5. Implement BookingService

### Booking Creation

- Validate Room existence
- Validate time constraints
- Apply conflict detection:

  (new_start < existing_end) AND (new_end > existing_start)

- Return 409 if conflict
- Auto-set Status = Pending

### Booking Update

- Prevent RoomId modification
- Prevent Status modification via PUT
- Revalidate conflict
- Update UpdatedAt

### Booking Delete

- Implement soft delete
- Set DeletedAt
- Return 404 if already deleted

## Endpoints

- POST /api/bookings
- GET /api/bookings
- GET /api/bookings/{id}
- PUT /api/bookings/{id}
- DELETE /api/bookings/{id}

## Completion Criteria

- Conflict detection works
- Soft delete enforced
- Deleted records never returned
- All validation rules enforced

---

# 6. Phase 5 – Status Management

## Goal
Implement separate status update endpoint.

## Implementation Steps

- Implement PATCH /api/bookings/{id}/status
- Validate enum:
  - Pending
  - Approved
  - Rejected
- Only update Status and UpdatedAt

## Completion Criteria

- Invalid enum returns 400
- Soft-deleted booking returns 404
- Status update isolated from general update endpoint

---

# 7. Phase 6 – Filtering & Pagination

## Goal
Enable advanced query support for bookings.

## Filters to Implement

- building_id (via join)
- room_id
- status
- borrower_name (partial match)
- start_date / end_date overlap

## Sorting

Allowed fields:
- created_at
- booking_start
- booking_end
- status

## Pagination

- Default page = 1
- Default page_size = 10
- Max page_size = 50

## Rules

- Filtering must occur at database level
- No in-memory filtering
- Apply filtering before pagination
- Apply pagination after sorting

## Completion Criteria

- Filters work independently
- Filters work combined
- Pagination metadata correct
- Performance acceptable

---

# 8. Phase 7 – Documentation & Release

## Tasks

- Finalize README.md
- Create CHANGELOG.md
- Verify all issues closed
- Create tag v1.0.0
- Push tag to GitHub

## Completion Criteria

- All tasks in Project Board moved to Done
- Release visible in repository
- Changelog reflects implemented features

---

# 9. Development Rules

- Do not implement multiple modules simultaneously.
- Complete one module before moving to next.
- Always review AI-generated code.
- Always compare implementation against AI_RULES.md.
- Never expand scope beyond PRD.

---

# 10. Definition of Done (Global)

A phase is complete when:

- Code merged via Pull Request
- Acceptance criteria satisfied
- Migration tested
- No TODO comments left
- Conventional commit used
- Issue closed and linked