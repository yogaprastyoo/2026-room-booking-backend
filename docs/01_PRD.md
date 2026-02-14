# Room Booking System (PRD)

**Project Name:** Room Booking System

**Scope:** Backend API ([ASP.NET](http://asp.net/) Core .NET 10)

**Target Environment:** Campus Internal System

**Version**: 1.0 (MVP)

# 1. Executive Summary

The Room Booking System is a backend service designed to manage campus room reservations in a structured and centralized way.

The current booking process is manual and unstructured, leading to:

- Inconsistent booking records
- No centralized data management
- No clear booking status tracking
- No structured historical retrieval

This backend provides a RESTful API to manage:

- Buildings
- Rooms
- Bookings
- Booking status lifecycle
- Search and filtering

The system is designed for web and mobile clients.

---

# 2. Product Scope

## 2.1 In Scope (MVP)

The system must support:

1. Building management (CRUD)
2. Room management (CRUD)
3. Booking management (CRUD)
4. Booking status update
5. Soft deletion (Booking only)
6. Search, filtering, and pagination
7. Conflict prevention (no double booking for same room & time range)

---

## 2.2 Out of Scope

The following features are explicitly excluded:

- Authentication & authorization
- Role-based access control
- Notifications
- Reporting dashboard
- Payment handling
- Integration with external systems

---

# 3. Domain Model Overview

The domain hierarchy is:

```
Building → Room → Booking
```

Each Booking is associated with exactly one Room.

Each Room belongs to exactly one Building.

---

# 4. Entity Definitions

---

# 4.1 Building

Represents a physical building.

## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| Id | integer | Yes | Primary key |
| Name | string | Yes | Unique building name |
| Code | string | Yes | Short building code |
| CreatedAt | datetime | Yes | Auto-generated |
| UpdatedAt | datetime | Yes | Auto-updated |

## Business Rules

1. Name must be unique.
2. Code must be unique.
3. Name maximum length: 100 characters.
4. Code maximum length: 20 characters.
5. Building cannot be deleted if it has any associated rooms (return HTTP 409 Conflict).

---

# 4.2 Room

Represents a room inside a building.

## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| Id | integer | Yes | Primary key |
| BuildingId | integer | Yes | Foreign key to Building |
| Name | string | Yes | Room name/number |
| Capacity | integer | No | Maximum occupancy |
| CreatedAt | datetime | Yes | Auto-generated |
| UpdatedAt | datetime | Yes | Auto-updated |

## Business Rules

1. (BuildingId + Name) must be unique.
2. Name maximum length: 100 characters.
3. Capacity must be positive if provided.
4. Room cannot be deleted if it has any active bookings (where DeletedAt is null). Return HTTP 409 Conflict. Note: Database FK constraint may also prevent deletion if soft-deleted bookings exist.

---

# 4.3 Booking

Represents a reservation request for a room.

## Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| Id | integer | Yes | Primary key |
| RoomId | integer | Yes | Foreign key to Room |
| BorrowerName | string | Yes | Requester name |
| BookingStart | datetime (UTC) | Yes | Start time (UTC) |
| BookingEnd | datetime (UTC) | Yes | End time (UTC) |
| Notes | string | No | Optional information |
| Status | enum | Yes | Pending / Approved / Rejected |
| CreatedAt | datetime (UTC) | Yes | System generated |
| UpdatedAt | datetime (UTC) | Yes | Auto-updated |
| DeletedAt | datetime (nullable) | No | Soft delete marker |

---

# 5. Booking Business Rules

---

## 5.1 Creation Rules

When creating a booking:

1. RoomId must reference an existing Room.
2. BorrowerName must not be null or empty.
3. BorrowerName maximum length: 100 characters.
4. BookingStart must be greater than or equal to current UTC time (>= now is acceptable).
5. BookingEnd must be strictly greater than BookingStart.
6. Booking duration must not be zero.
7. Status must automatically be set to `Pending`.
8. Client must not provide:
    - Id
    - Status
    - CreatedAt
    - UpdatedAt
    - DeletedAt
9. CreatedAt and UpdatedAt must be set by the system.
10. DeletedAt must be null.

If any rule is violated → return HTTP 400.

---

## 5.2 Conflict Prevention Rule

The system must prevent overlapping bookings for the same Room.

A booking conflict exists if:

```
(new_start < existing_end) AND (new_end > existing_start)
```

Conditions:

- Only consider bookings where DeletedAt is null.
- Conflict detection applies regardless of Status value (including Rejected bookings).
- Conflict check must apply during:
    - Booking creation
    - Booking time update

If conflict exists:

- Return HTTP 409 Conflict.
- Do not persist changes.

---

## 5.3 Update Rules

1. RoomId cannot be changed after creation.
2. Status cannot be changed via general update endpoint.
3. BookingStart and BookingEnd must still satisfy time validation.
4. Conflict rule must be revalidated if time is updated.
5. UpdatedAt must be updated automatically.
6. If booking is soft-deleted, return 404.

---

## 5.4 Status Management Rules

Allowed values:

- Pending
- Approved
- Rejected

Rules:

1. Status must default to `Pending` upon creation.
2. Status may only be modified via the dedicated status update endpoint.
3. All transitions between valid status values are allowed.
4. There are no terminal states.
5. Invalid enum values must return HTTP 400.

---

## 5.5 Soft Delete Rules

1. Booking deletion must be soft delete.
2. DeletedAt must be set to current timestamp.
3. Soft-deleted bookings must not appear in list queries.
4. Deleting already deleted record returns 404.

---

# 6. Functional Requirements

---

## 6.1 Building Management

The system must support:

- Create building
- Retrieve building list
- Retrieve building detail
- Update building
- Delete building (if no rooms)

Pagination required for list endpoint.

---

## 6.2 Room Management

The system must support:

- Create room
- Retrieve room list
- Retrieve room detail
- Update room
- Delete room (if no bookings)

Filtering:

- By BuildingId
- By room name

Pagination required.

---

## 6.3 Booking Management

The system must support:

- Create booking
- Retrieve booking list
- Retrieve booking detail
- Update booking
- Delete booking (soft)
- Update booking status

Filtering:

- By building
- By room
- By status
- By date range
- By borrower name

Pagination required.

---

# 7. Non-Functional Requirements

---

## 7.1 Architecture

The system must:

- Follow OOP principles
- Separate Controller, Service, and Data layer
- Use DTO-based validation
- Contain no business logic inside controllers
- Use consistent response envelope

---

## 7.2 Performance

- CRUD operations must respond in < 500ms under normal load.
- Conflict detection must be database-optimized.

---

## 7.3 Data Integrity

- Foreign key constraints enforced.
- Enum values strictly validated.
- Soft delete implemented via query filter.

---

## 7.4 Versioning

- Semantic Versioning
- Initial tag: v1.0.0

---

# 8. Acceptance Criteria (MVP Completion)

The backend is considered complete when:

1. All CRUD endpoints function correctly.
2. Conflict prevention works.
3. Status transition rules are enforced.
4. Soft delete works.
5. Pagination and filtering work.
6. No business logic exists in controllers.
7. API response structure is consistent.
8. Project tagged with v1.0.0.

---

# 9. Assumptions

- Single campus.
- Single timezone (UTC internally).
- No high concurrency edge cases required for MVP.
- No authentication required.

[Feature 01: Booking Management](Feature%2001%20Booking%20Management%203051af628d2a801699b0d476197980a6.md)

[**Feature 02: Booking Status Management**](Feature%2002%20Booking%20Status%20Management%203051af628d2a8094b319c0e97561d810.md)

[Feature 03: Search, Filtering, and Pagination](Feature%2003%20Search,%20Filtering,%20and%20Pagination%203051af628d2a8000b70ef89e6cc00615.md)

[JSON data contracts for Building](JSON%20data%20contracts%20for%20Building%203051af628d2a80fb8ffdd8034d326921.md)

[Database Relationship Specification](Database%20Relationship%20Specification%203051af628d2a804abe0fdc8daf956237.md)