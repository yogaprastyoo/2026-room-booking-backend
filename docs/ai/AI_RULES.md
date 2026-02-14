# AI Implementation Rules – Room Booking Backend (PBL 2026)

This document defines strict implementation rules for AI-assisted development.
All generated code must comply with these constraints.

---

## 1. Architecture Rules

- Use single-project layered architecture.
- Folder structure inside project must be:

  Controllers/
  Services/
  Domain/
  DTOs/
  Data/
  Mappings/

- No business logic inside Controllers.
- Controllers must only:
  - Receive request
  - Validate DTO
  - Call Service
  - Return standardized response

- All business logic must be placed inside Services.
- Domain entities must not depend on EF Core.

---

## 2. API Rules

- All responses must follow standardized response envelope:

  {
    "success": true,
    "message": "...",
    "data": {}
  }

- Use snake_case for all JSON fields.
- Use DTO-based validation.
- Use proper HTTP status codes:
  - 200 OK
  - 201 Created
  - 204 No Content
  - 400 Bad Request
  - 404 Not Found
  - 409 Conflict

---

## 3. Database Rules (PostgreSQL)

- Use Npgsql.EntityFrameworkCore.PostgreSQL.
- Configure constraints using Fluent API.
- Enforce:
  - UNIQUE(building_id, name) for rooms
  - CHECK (booking_end > booking_start)
- Use PostgreSQL `timestamptz` type for all timestamp columns.
- Configure EF Core to map entity properties to snake_case database column names.
- All timestamps must be stored in UTC.
- No hardcoded connection string.
- Use environment variable for DB connection.

---

## 4. Booking Business Rules

- Status enum values:
  Pending
  Approved
  Rejected

- Status auto-set to Pending on creation.
- Status must not be updated via PUT endpoint.
- Conflict detection rule:

  (new_start < existing_end) AND (new_end > existing_start)

- Conflict must return HTTP 409.
- Conflict applies to all non soft-deleted bookings (regardless of Status, including Rejected).

---

## 5. Soft Delete Rules

- Soft delete applies only to Booking.
- Use DeletedAt (nullable timestamp).
- Soft-deleted records must never appear in queries.
- Implement global query filter in DbContext.

---

## 6. Filtering Rules

- Filtering must be database-level.
- Do not use in-memory filtering.
- Do not call ToList() before applying filters.
- Apply filtering before pagination.
- Apply pagination after sorting.

---

## 7. Coding Rules

- Use async/await for all database operations.
- No synchronous database calls.
- No business logic in Controllers.
- Keep methods small and focused.
- Use meaningful naming.
- Follow conventional commit when committing.

---

## 8. Scope Constraints (MVP)

- No authentication.
- No authorization.
- No notification system.
- No status history tracking.
- No advanced state machine.
- Do not add features outside PRD scope.