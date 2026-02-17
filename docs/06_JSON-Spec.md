# JSON data contracts for Building

# 1. Global API Response Envelope

All API responses MUST follow this structure.

## 1.1 Success Response

```
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": {},
  "errors": null
}
```

## 1.2 Validation Error Response

```
{
  "success": false,
  "message": "Validation failed.",
  "data": null,
  "errors": {
    "field_name": ["Error message 1", "Error message 2"]
  }
}
```

## 1.3 Generic Error Response

```
{
  "success": false,
  "message": "Resource not found.",
  "data": null,
  "errors": null
}
```

---

# 2. Building Data Contracts

---

## 2.1 Building Resource Representation

```
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Engineering Building",
  "code": "ENG",
  "created_at": "2026-02-01T08:00:00Z",
  "updated_at": "2026-02-01T08:00:00Z"
}
```

---

## 2.2 Create Building

### Request

**POST /api/buildings**

```
{
  "name": "Engineering Building",
  "code": "ENG"
}
```

### Constraints

- name: required, max 100 characters, unique
- code: required, max 20 characters, unique
- Client must not send id, created_at, updated_at

---

### Success Response (201)

```
{
  "success": true,
  "message": "Building created successfully.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Engineering Building",
    "code": "ENG",
    "created_at": "2026-02-01T08:00:00Z",
    "updated_at": "2026-02-01T08:00:00Z"
  }
}
```

---

## 2.3 Update Building

**PUT /api/buildings/{id}**

### Request

```
{
  "name": "Updated Building Name",
  "code": "ENG2"
}
```

Constraints identical to creation.

---

## 2.4 Retrieve Building List

**GET /api/buildings**

### Success Response

```
{
  "success": true,
  "message": "Buildings retrieved successfully.",
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Engineering Building",
        "code": "ENG",
        "created_at": "2026-02-01T08:00:00Z",
        "updated_at": "2026-02-01T08:00:00Z"
      }
    ],
    "pagination": {
      "items": [], 
      "total_items": 1,
      "total_pages": 1,
      "current_page": 1,
      "page_size": 10
    }
  }
}
```

*(Note: Pagination structure flat in data or nested? Code uses `PagedResult<T>` which has `Items`, `TotalItems` etc. StandardApiResponse wraps this PagedResult object as `Data`.)*

Correct structure based on code:

```
{
  "success": true,
  "message": "Buildings retrieved successfully.",
  "data": {
    "items": [ ... ],
    "total_items": 1,
    "total_pages": 1,
    "current_page": 1,
    "page_size": 10
  }
}
```

---

# 3. Room Data Contracts

---

## 3.1 Room Resource Representation

```
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "building_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Room 101",
  "capacity": 50,
  "created_at": "2026-02-01T09:00:00Z",
  "updated_at": "2026-02-01T09:00:00Z"
}
```

---

## 3.2 Create Room

**POST /api/rooms**

### Request

```
{
  "building_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Room 101",
  "capacity": 50
}
```

### Constraints

- building_id: required, must exist
- name: required, max 100 characters
- capacity: optional, must be positive integer if provided
- (building_id + name) must be unique
- Client must not send id, created_at, updated_at

---

### Success Response (201)

```
{
  "success": true,
  "message": "Room created successfully.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "building_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Room 101",
    "capacity": 50,
    "created_at": "2026-02-01T09:00:00Z",
    "updated_at": "2026-02-01T09:00:00Z"
  }
}
```

---

## 3.3 Retrieve Room List

**GET /api/rooms**

Supported filter:

- page
- page_size

### Success Response

```
{
  "success": true,
  "message": "Rooms retrieved successfully.",
  "data": {
    "items": [ ... ],
    "total_items": 10,
    "total_pages": 1,
    "current_page": 1,
    "page_size": 10
  }
}
```

---

# 4. Booking Data Contracts

---

## 4.1 Booking Resource Representation

```
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "room_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "borrower_name": "John Doe",
  "booking_start": "2026-02-15T09:00:00Z",
  "booking_end": "2026-02-15T11:00:00Z",
  "notes": "Weekly seminar",
  "status": "Pending",
  "created_at": "2026-02-01T08:00:00Z",
  "updated_at": "2026-02-01T08:00:00Z"
}
```

The deleted_at field is internal and must never be returned in API responses.

---

## 4.2 Create Booking

**POST /api/bookings**

### Request

```
{
  "room_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "borrower_name": "John Doe",
  "booking_start": "2026-02-15T09:00:00Z",
  "booking_end": "2026-02-15T11:00:00Z",
  "notes": "Weekly seminar"
}
```

### Constraints

- room_id: required, must exist
- borrower_name: required, max 100
- booking_start: required, ISO 8601 UTC, ≥ now
- booking_end: required, > booking_start
- notes: optional, max 500
- Client must not send status, created_at, updated_at
- Conflict detection must be enforced (Block if Approved overlaps)
- Status auto-set to "Pending"

---

### Success Response (201)

```
{
  "success": true,
  "message": "Booking created successfully.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "room_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "borrower_name": "John Doe",
    "booking_start": "2026-02-15T09:00:00Z",
    "booking_end": "2026-02-15T11:00:00Z",
    "notes": "Weekly seminar",
    "status": "Pending",
    "created_at": "2026-02-01T08:00:00Z",
    "updated_at": "2026-02-01T08:00:00Z"
  }
}
```

---

## 4.3 Update Booking

**PUT /api/bookings/{id}**

### Request

```
{
  "borrower_name": "John Doe",
  "booking_start": "2026-02-15T10:00:00Z",
  "booking_end": "2026-02-15T12:00:00Z",
  "notes": "Updated seminar"
}
```

Constraints:

- room_id must not be present
- status must not be present
- Conflict rule must be re-evaluated

---

## 4.4 Update Booking Status

**PATCH /api/bookings/{id}/status**

### Request

```
{
  "status": "Approved"
}
```

Allowed values:

```
["Pending", "Approved", "Rejected"]
```

Invalid enum value → 400.

---

## 4.5 Retrieve Booking List

**GET /api/bookings**

Supported filters:

- building_id
- room_id
- status
- borrower_name
- start_date
- end_date
- page
- page_size

### Success Response

```
{
  "success": true,
  "message": "Bookings retrieved successfully.",
  "data": {
    "items": [ /* booking objects */ ],
    "total_items": 37,
    "total_pages": 4,
    "current_page": 1,
    "page_size": 10
  }
}
```

---

# 5. Strict Consistency Rules

1. All timestamps must use ISO 8601 UTC format.
2. All JSON fields must use snake_case.
3. Enum values are case-sensitive.
4. Soft-deleted bookings must never be returned.
5. All responses must follow the standardized envelope.
6. Conflict detection must return HTTP 409.
7. Foreign key integrity must be enforced.