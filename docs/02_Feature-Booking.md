# Feature 01: Booking Management

## 1. General Description

The Booking Management feature is responsible for handling the complete lifecycle of room reservation records within the system.

This feature operates on a normalized domain structure:

```
Building → Room → Booking
```

Each Booking:

- Must reference a valid Room.
- Indirectly belongs to a Building through its associated Room.
- Represents a reservation within a specific time range.

This feature provides:

- Creation of booking requests
- Retrieval of booking records
- Modification of booking details
- Controlled status management (separate endpoint)
- Soft deletion of booking records
- Conflict prevention (no overlapping reservations)
- Enforcement of domain-level business rules

Authentication and authorization are explicitly out of scope for MVP.

---

## 2. Domain Entity Definition

### Entity Name: `Booking`

A Booking represents a reservation request for a specific Room within a defined time interval.

### Data Structure

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| Id | integer | Yes | Unique identifier (Primary Key) |
| RoomId | integer | Yes | Foreign key referencing Room |
| BorrowerName | string | Yes | Name of the requesting person |
| BookingStart | datetime (UTC) | Yes | Reservation start time |
| BookingEnd | datetime (UTC) | Yes | Reservation end time |
| Notes | string | No | Optional additional information |
| Status | enum | Yes | Pending / Approved / Rejected |
| CreatedAt | datetime | Yes | Timestamp of creation |
| UpdatedAt | datetime | Yes | Timestamp of last modification |
| DeletedAt | datetime (nullable) | No | Soft deletion marker |

---

## 3. Building and Room Dependency Requirements

Although Building and Room are separate features, Booking depends on them.

### Room Requirements

- RoomId must reference an existing Room.
- The referenced Room must not be deleted.
- A Room belongs to exactly one Building.
- Room cannot be changed after booking creation.

### Building Context

- Booking does not store BuildingId directly.
- Building context is derived from the Room relationship.
- Filtering by Building must be supported via Room join.

---

## 4. Business Rules

---

## 4.1 Creation Rules

When creating a booking:

1. RoomId must exist.
2. BorrowerName must not be null or empty.
3. BorrowerName maximum length: 100 characters.
4. BookingStart must be greater than or equal to current UTC time (>= now is acceptable).
5. BookingEnd must be greater than BookingStart.
6. Notes maximum length: 500 characters.
7. Status must automatically be set to `Pending`.
8. Client must not provide Status.
9. CreatedAt and UpdatedAt must be set by the system.
10. DeletedAt must be null.

If any validation rule fails, return HTTP 400.

---

## 4.2 Conflict Prevention Rule

The system must prevent overlapping bookings for the same Room.

A conflict exists if:

```
(new_start < existing_end) AND (new_end > existing_start)
```

Conflict evaluation must:

- **Only consider bookings where Status is `Approved`.**
- Bookings with Status `Pending` or `Rejected` do **not** block the time slot.
- Exclude bookings where DeletedAt != null (soft-deleted).
- Exclude the current booking when updating (self-overlap check).

If conflict is detected:

- Return HTTP 409 Conflict.
- Do not persist the booking.

---

## 4.3 Update Rules

When updating a booking:

1. RoomId must not be modified.
2. Status must not be modified via the general update endpoint.
3. BorrowerName may be updated.
4. BookingStart and BookingEnd may be updated.
5. Time validation rules must be re-evaluated.
6. Conflict rule must be re-evaluated.
7. UpdatedAt must automatically be updated.
8. CreatedAt must remain unchanged.
9. DeletedAt must remain unchanged.
10. If booking does not exist or is soft-deleted, return 404.

---

## 4.4 Status Management Separation

Status must not be modified via the general update endpoint.

Status updates are handled through a dedicated endpoint defined in Feature 02.

---

## 4.5 Soft Delete Rules

When deleting a booking:

1. The record must not be physically removed.
2. DeletedAt must be set to current UTC timestamp.
3. UpdatedAt must also be updated.
4. Soft-deleted records must not appear in default queries.
5. If record does not exist or already soft-deleted, return 404.

---

## 5. API Behavior Specification

---

### 5.1 Create Booking

Method: POST

Endpoint: `/api/bookings`

Behavior:

- Validate input.
- Validate Room existence.
- Validate time range.
- Validate conflict rule.
- Persist booking.
- Return HTTP 201.
- Return created booking object including Room and Building context.

---

### 5.2 Retrieve Booking List

Method: GET

Endpoint: `/api/bookings`

Behavior:

- Exclude soft-deleted records.
- Support filtering by:
    - building_id
    - room_id
    - status
    - borrower_name (partial match)
    - date range (start/end overlap)
- Apply filtering before pagination.
- Apply pagination (Max page size: 50).
- Default sorting: CreatedAt descending.
- Include Room and Building context in response.

---

### 5.3 Retrieve Booking Detail

Method: GET

Endpoint: `/api/bookings/{id}`

Behavior:

- Return 404 if not found or soft-deleted.
- Return full booking object including:
    - Room information
    - Building information

---

### 5.4 Update Booking

Method: PUT

Endpoint: `/api/bookings/{id}`

Behavior:

- Validate update rules.
- Revalidate time constraints.
- Revalidate conflict rule.
- Return updated booking object.
- Return 404 if not found.

---

### 5.5 Delete Booking

Method: DELETE

Endpoint: `/api/bookings/{id}`

Behavior:

- Perform soft delete.
- Return HTTP 204.
- No response body.

---

## 6. Validation Constraints

The following constraints must be enforced:

- BorrowerName: max 100 characters.
- Notes: max 500 characters.
- BookingStart and BookingEnd must use ISO 8601 UTC format.
- BookingStart < BookingEnd.
- RoomId must be valid integer.
- Status must use predefined enum values only.
- RoomId must not change after creation.

---

## 7. Error Handling Requirements

The API must return consistent error responses.

| Scenario | HTTP Status |
| --- | --- |
| Validation failure | 400 |
| Conflict detected | 409 |
| Not found | 404 |
| Internal server error | 500 |

---

## 8. Completion Criteria

This feature is considered complete when:

1. All CRUD operations function correctly.
2. Room and Building relationships are enforced.
3. Conflict prevention is operational for `Approved` bookings.
4. Soft deletion is enforced.
5. Filtering and pagination work.
6. Status cannot be modified via general update endpoint.
7. No business logic exists inside controllers.
8. All responses follow the standardized API envelope format.