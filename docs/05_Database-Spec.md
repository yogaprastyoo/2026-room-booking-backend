# Database Relationship Specification

# 1. Entity Relationship Overview

The system follows this hierarchy:

```
Building (1) ────< (N) Room (1) ────< (N) Booking
```

Meaning:

- One Building has many Rooms.
- One Room belongs to one Building.
- One Room has many Bookings.
- One Booking belongs to one Room.

There is **no direct foreign key from Booking to Building**.

---

# 2. Table Specifications

---

# 2.1 Table: buildings

## Columns

| Column | Type | Nullable | Constraints |
| --- | --- | --- | --- |
| id | bigint | No | Primary Key, Identity |
| name | varchar(100) | No | Unique |
| code | varchar(20) | No | Unique |
| created_at | timestamptz | No | Default CURRENT_TIMESTAMP |
| updated_at | timestamptz | No | Default CURRENT_TIMESTAMP |

---

## Constraints

- PRIMARY KEY (id)
- UNIQUE (name)
- UNIQUE (code)

---

## Indexes

- Unique index on name
- Unique index on code

---

# 2.2 Table: rooms

## Columns

| Column | Type | Nullable | Constraints |
| --- | --- | --- | --- |
| id | bigint | No | Primary Key, Identity |
| building_id | bigint | No | Foreign Key |
| name | varchar(100) | No |  |
| capacity | integer | Yes | Must be > 0 if not null |
| created_at | timestamptz | No | Default CURRENT_TIMESTAMP |
| updated_at | timestamptz | No | Default CURRENT_TIMESTAMP |

---

## Foreign Key

```
rooms.building_id → buildings.id
```

ON DELETE RESTRICT

ON UPDATE CASCADE

Meaning:

- A building cannot be deleted if rooms exist.
- If building id changes (rare), propagate.

---

## Unique Constraint

```
UNIQUE (building_id, name)
```

This prevents duplicate room names inside the same building.

---

## Indexes

- Index on building_id
- Unique composite index (building_id, name)

---

# 2.3 Table: bookings

## Columns

| Column | Type | Nullable | Constraints |
| --- | --- | --- | --- |
| id | bigint | No | Primary Key, Identity |
| room_id | bigint | No | Foreign Key |
| borrower_name | varchar(100) | No |  |
| booking_start | timestamptz | No |  |
| booking_end | timestamptz | No |  |
| notes | varchar(500) | Yes |  |
| status | varchar(20) | No | Enum constraint |
| created_at | timestamptz | No | Default CURRENT_TIMESTAMP |
| updated_at | timestamptz | No | Default CURRENT_TIMESTAMP |
| deleted_at | timestamptz | Yes | Soft delete marker |

---

## Foreign Key

```
bookings.room_id → rooms.id
```

ON DELETE RESTRICT

ON UPDATE CASCADE

Meaning:

- A room cannot be deleted if bookings exist.
- Prevents orphan bookings.

---

## Enum Constraint (Database-Level)

Allowed values:

```
Pending
Approved
Rejected
```

Implementation options:

- CHECK constraint
- Native ENUM (PostgreSQL)
- VARCHAR + CHECK (SQL Server)

Example constraint:

```
CHECK (status IN ('Pending', 'Approved', 'Rejected'))
```

---

# 3. Conflict Prevention Logic (Database Perspective)

Conflict detection is an application-level rule, not a pure constraint.

To support conflict detection queries, create a composite index:

```
INDEX idx_booking_room_time
ON bookings (room_id, booking_start, booking_end)
```

This improves overlap detection performance.

---

# 4. Soft Delete Specification

Soft delete applies only to:

- bookings

Implementation:

- deleted_at IS NULL → active record
- deleted_at IS NOT NULL → soft deleted

All SELECT queries for bookings must include:

```
WHERE deleted_at IS NULL
```

This must be enforced globally (query filter / ORM-level filter).

---

# 5. Referential Integrity Rules

1. Building cannot be deleted if rooms exist.
2. Room cannot be deleted if active bookings exist (DeletedAt IS NULL). FK constraint may also prevent deletion if soft-deleted bookings exist (acceptable for MVP).
3. Booking cannot exist without room.
4. Booking cannot exist without building (indirectly enforced).
5. Cascading physical deletes are not allowed.
6. Only Booking supports soft delete.

---

# 6. Time Integrity Constraints

Database-level check:

```
CHECK (booking_end > booking_start)
```

Optional (recommended).

Application must enforce:

- booking_start >= current timestamp

---

# 7. Indexing Strategy

To ensure MVP performance:

### Buildings

- Unique index on name
- Unique index on code

### Rooms

- Index on building_id
- Unique composite (building_id, name)

### Bookings

- Index on room_id
- Index on status
- Index on booking_start
- Composite index (room_id, booking_start, booking_end) for conflict detection

---

# 8. Transaction Requirements

Create and Update Booking must be executed inside a transaction to:

1. Validate conflict.
2. Insert/update record.

For MVP, standard transaction isolation (READ COMMITTED) is acceptable.

---

# 9. Data Normalization Justification

This schema satisfies:

- 1NF (atomic values)
- 2NF (no partial dependency)
- 3NF (no transitive dependency)

No redundant building name stored in bookings.

Building context is derived via join.

---

# 10. ERD Summary (Logical View)

```
[buildings]
  id (PK)
  name (UQ)
  code (UQ)

        |
        | 1:N
        v

[rooms]
  id (PK)
  building_id (FK → buildings.id)
  name
  capacity
  UNIQUE(building_id, name)

        |
        | 1:N
        v

[bookings]
  id (PK)
  room_id (FK → rooms.id)
  borrower_name
  booking_start
  booking_end
  status
  deleted_at
```

---

# 11. Completion Criteria

The database specification is considered complete when:

1. All foreign keys are enforced.
2. Unique constraints are enforced.
3. Enum constraint is enforced.
4. booking_end > booking_start constraint exists.
5. Indexes exist for conflict detection.
6. Soft delete filter is enforced at query level.
7. No redundant columns exist.

---

This database relationship specification is now:

- Fully normalized
- Strictly scoped
- MVP-aligned
- Conflict-aware
- AI-agent ready
- Migration-ready
- Production-structured