# Feature 03: Search, Filtering, and Pagination

## 1. General Description

The Search and Filtering feature enables controlled retrieval of Booking records using structured query parameters.

This feature must:

- Support filtering across Building, Room, Status, Borrower, and Time Range.
- Exclude soft-deleted records.
- Apply filtering before pagination.
- Return consistent metadata for pagination.
- Be deterministic and side-effect free.

This feature applies only to Booking retrieval endpoints.

---

## 2. Scope

This feature applies to:

```
GET /api/bookings
```

It does not apply to:

- POST
- PUT
- PATCH
- DELETE
- Building list endpoints
- Room list endpoints (unless separately defined)

---

## 3. Filtering Capabilities

Filtering must support the following parameters.

All parameters are optional.

---

## 3.1 Building Filter

Query parameter:

```
building_id
```

Behavior:

- Return only bookings where:
    - Booking → Room → BuildingId matches provided building_id.
- Invalid building_id format (non-integer) → return 400.
- Must not return 404 for non-existing building_id.

---

## 3.2 Room Filter

Query parameter:

```
room_id
```

Behavior:

- Return only bookings where RoomId equals room_id.
- If room_id does not exist, return empty result set.
- Must not return 404.

---

## 3.3 Status Filter

Query parameter:

```
status
```

Allowed values:

```
Pending
Approved
Rejected
```

Behavior:

- Must match enum exactly.
- If invalid value provided → return 400.
- Filter applied directly on Booking.Status.

---

## 3.4 Borrower Name Search

Query parameter:

```
borrower_name
```

Behavior:

- Perform case-insensitive partial match.
- Must use LIKE semantics (contains).
- Example:
    - borrower_name=jo → matches “John”, “Jordan”.

---

## 3.5 Time Range Filter

Query parameters:

```
start_date
end_date
```

Behavior:

This filter returns bookings that overlap with the provided date range.

Overlap logic:

```
(booking_start < filter_end) AND (booking_end > filter_start)
```

Rules:

1. If only start_date provided:
    - Return bookings where booking_end > start_date.
2. If only end_date provided:
    - Return bookings where booking_start < end_date.
3. If both provided:
    - Apply full overlap rule.
4. Dates must be ISO 8601 UTC format.
5. If start_date > end_date → return 400.

---

## 4. Combined Filtering Logic

All filters must be combinable.

Filtering order:

1. Exclude records where DeletedAt IS NOT NULL.
2. Apply Building filter (via join).
3. Apply Room filter.
4. Apply Status filter.
5. Apply Borrower search.
6. Apply Time range filter.
7. Apply sorting.
8. Apply pagination.

The system must not ignore any valid filter parameter.

---

## 5. Sorting Rules

Default sorting:

```
CreatedAt DESC
```

Optional query parameter:

```
sort_by
sort_order
```

Allowed sort_by values:

```
created_at
booking_start
booking_end
status
```

Allowed sort_order:

```
asc
desc
```

If invalid sorting field provided → return 400.

---

## 6. Pagination Rules

Query parameters:

```
page
page_size
```

Rules:

1. Default page = 1.
2. Default page_size = 10.
3. Maximum page_size = 50.
4. page must be ≥ 1.
5. page_size must be between 1 and 50.
6. Pagination must be applied after filtering and sorting.
7. If requested page exceeds total_pages, return 200 OK with empty items array and valid pagination metadata.

---

## 7. Response Structure

Response must follow standardized envelope.

### Example Response

```
{
  "success": true,
  "message": "Bookings retrieved successfully.",
  "data": {
    "items": [
      {
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
    ],
    "pagination": {
      "page": 1,
      "page_size": 10,
      "total_count": 37,
      "total_pages": 4
    }
  }
}
```

---

## 8. Soft Delete Enforcement

Soft-deleted bookings:

- Must never appear in list results.
- Must not affect total_count.
- Must not appear even if filter matches.

This rule is mandatory.

---

## 9. Error Handling

| Scenario | HTTP Status |
| --- | --- |
| Invalid enum value | 400 |
| Invalid date format | 400 |
| start_date > end_date | 400 |
| Invalid sort field | 400 |
| Invalid pagination values | 400 |
| Internal error | 500 |

---

## 10. Performance Constraints

1. Filtering must be database-level.
2. Joins (Booking → Room → Building) must be optimized.
3. Pagination must use skip/take or equivalent.
4. No in-memory filtering for large datasets.

---

## 11. Completion Criteria

This feature is considered complete when:

1. All filters work independently.
2. Filters work correctly in combination.
3. Time overlap logic works correctly.
4. Soft-deleted records are excluded.
5. Pagination metadata is accurate.
6. Sorting works and validates input.
7. Response format is consistent.
8. No filtering logic exists in controller layer.