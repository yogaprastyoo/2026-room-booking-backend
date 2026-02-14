# Feature 02: Booking Status Management

## 1. General Description

The Booking Status Management feature controls the lifecycle state of a Booking.

Status modification must:

- Be isolated from general update operations.
- Follow strict transition rules.
- Not modify any other booking fields.
- Respect soft deletion rules.

This feature ensures that booking approval or rejection follows controlled domain logic.

Authentication and role validation are out of scope for MVP.

---

## 2. Status Definition

Status must be implemented as a strict enum.

Allowed values:

```
Pending
Approved
Rejected
```

Rules:

1. Enum values are case-sensitive.
2. No other values are permitted.
3. Status must always have a value.
4. Default status upon creation is `Pending`.

---

## 3. Status Lifecycle Model

A booking follows a flexible state model within the MVP scope.

Initial state:

```
Pending
```

Allowed transitions:

```
Pending → Approved
Pending → Rejected
Approved → Pending
Approved → Rejected
Rejected → Pending
Rejected → Approved
```

All transitions between valid enum values are allowed.

There are no terminal states in MVP scope.

If an invalid enum value is provided, the system must return HTTP 400.

---

## 4. Preconditions for Status Update

Before updating status, the system must validate:

1. Booking exists.
2. Booking is not soft-deleted (DeletedAt must be null).
3. New status is a valid enum value.

If any condition fails:

- Return 404 if booking does not exist or is soft-deleted.
- Return 400 if status value is invalid.

---

## 5. Conflict Rule Interaction

Status update must not bypass booking conflict logic.

Important clarification:

- Conflict prevention applies only during creation or time updates.
- Status update does NOT re-evaluate time conflicts.
- Status update does NOT modify BookingStart or BookingEnd.

This prevents unintended side effects.

---

## 6. API Specification

---

### 6.1 Update Booking Status

Method: PATCH

Endpoint: `/api/bookings/{id}/status`

---

### Request Body

```
{
  "status": "Approved"
}
```

Constraints:

- Request must contain only the `status` field.
- Any additional fields must result in HTTP 400.
- Status must match enum values exactly.

---

### Success Response (200)

```
{
  "success": true,
  "message": "Booking status updated successfully.",
  "data": {
    "id": 12,
    "room": {
      "id": 5,
      "name": "Room 101",
      "building": {
        "id": 2,
        "name": "Engineering Building",
        "code": "ENG"
      }
    },
    "borrower_name": "John Doe",
    "booking_start": "2026-02-15T09:00:00Z",
    "booking_end": "2026-02-15T11:00:00Z",
    "notes": null,
    "status": "Approved",
    "created_at": "2026-02-01T08:00:00Z",
    "updated_at": "2026-02-03T08:00:00Z"
  }
}
```

---

## 7. System-Level Constraints

1. Status update must only modify:
    - Status
    - UpdatedAt
2. All other fields must remain unchanged.
3. RoomId must not change.
4. BookingStart and BookingEnd must not change.
5. CreatedAt must not change.
6. DeletedAt must not change.

---

## 8. Error Handling

| Scenario | HTTP Status |
| --- | --- |
| Booking not found | 404 |
| Booking soft-deleted | 404 |
| Invalid enum value | 400 |
| Request body malformed | 400 |
| Internal server error | 500 |

---

## 9. Completion Criteria

This feature is considered complete when:

1. All transitions between valid enum values are allowed.
2. Invalid enum values return HTTP 400.
3. Soft-deleted bookings cannot be modified (return 404).
4. Only Status and UpdatedAt are modified.
5. Response includes Building and Room context.
6. API response follows the standardized envelope format.
7. No business logic exists in the controller layer.

---

## 10. Explicit Non-Goals (To Prevent Scope Creep)

The following are NOT included in MVP:

- Automatic expiration of Pending bookings.
- Automatic rejection after time passes.
- Role-based approval enforcement.
- Multi-stage approval.
- Status history tracking.

These can be introduced in future versions.