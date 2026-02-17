using Microsoft.EntityFrameworkCore;
using Npgsql;
using RoomBooking.Api.Data;
using RoomBooking.Api.Domain;
using RoomBooking.Api.DTOs.Booking;
using RoomBooking.Api.DTOs.Common;
using RoomBooking.Api.Services.Interfaces;

namespace RoomBooking.Api.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate and normalize inputs
        var roomId = ValidateRoomId(request.RoomId);
        var borrowerName = ValidateAndNormalizeBorrowerName(request.BorrowerName);
        ValidateBookingTimes(request.BookingStart, request.BookingEnd);
        var notes = ValidateAndNormalizeNotes(request.Notes);

        // Check for overlapping bookings (application-level rule per PRD)
        await ValidateNoOverlap(roomId, request.BookingStart, request.BookingEnd, null);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            BorrowerName = borrowerName,
            BookingStart = request.BookingStart,
            BookingEnd = request.BookingEnd,
            Notes = notes,
            Status = BookingStatus.Pending  // Auto-set to Pending per PRD
        };

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
        {
            if (pgEx.ConstraintName == "fk_bookings_room")
            {
                throw new InvalidOperationException("Room does not exist.");
            }

            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
        {
            if (pgEx.ConstraintName == "chk_bookings_end_after_start")
            {
                throw new ArgumentException("Booking end time must be after start time.");
            }

            throw;
        }

        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> GetByIdAsync(Guid id)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (booking == null)
        {
            return null;
        }

        return MapToResponse(booking);
    }

    public async Task<PagedResult<BookingResponse>> GetAllAsync(BookingFilterRequest filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        // Validate pagination (PRD max is 50, not 100)
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 10;
        if (filter.PageSize > 50) filter.PageSize = 50;

        // Validate sorting
        var validSortFields = new[] { "created_at", "booking_start", "booking_end", "status" };
        var sortBy = filter.SortBy?.ToLower() ?? "created_at";
        if (!validSortFields.Contains(sortBy))
        {
            throw new ArgumentException($"Invalid sort field. Allowed: {string.Join(", ", validSortFields)}");
        }

        var sortOrder = filter.SortOrder?.ToLower() ?? "desc";
        if (sortOrder != "asc" && sortOrder != "desc")
        {
            throw new ArgumentException("Invalid sort order. Allowed: asc, desc");
        }

        // Validate status enum
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (!Enum.TryParse<BookingStatus>(filter.Status, true, out _))
            {
                throw new ArgumentException("Invalid status value. Allowed: Pending, Approved, Rejected");
            }
        }

        // Validate time range
        if (filter.StartDate.HasValue && filter.EndDate.HasValue && filter.StartDate > filter.EndDate)
        {
            throw new ArgumentException("start_date cannot be greater than end_date");
        }

        // Build query with filters (order per PRD line 156-165)
        var query = _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.Building)
            .AsNoTracking();

        // 1. Soft delete already handled by global query filter

        // 2. Building filter (via join)
        if (filter.BuildingId.HasValue)
        {
            query = query.Where(b => b.Room.BuildingId == filter.BuildingId.Value);
        }

        // 3. Room filter
        if (filter.RoomId.HasValue)
        {
            query = query.Where(b => b.RoomId == filter.RoomId.Value);
        }

        // 4. Status filter
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var statusEnum = Enum.Parse<BookingStatus>(filter.Status, true);
            query = query.Where(b => b.Status == statusEnum);
        }

        // 5. Borrower search (case-insensitive partial match)
        if (!string.IsNullOrWhiteSpace(filter.BorrowerName))
        {
            var searchTerm = filter.BorrowerName.Trim();
            query = query.Where(b => EF.Functions.ILike(b.BorrowerName, $"%{searchTerm}%"));
        }

        // 6. Time range filter (overlap logic per PRD)
        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            // Full overlap: (booking_start < filter_end) AND (booking_end > filter_start)
            query = query.Where(b => b.BookingStart < filter.EndDate.Value && b.BookingEnd > filter.StartDate.Value);
        }
        else if (filter.StartDate.HasValue)
        {
            // Only start_date: booking_end > start_date
            query = query.Where(b => b.BookingEnd > filter.StartDate.Value);
        }
        else if (filter.EndDate.HasValue)
        {
            // Only end_date: booking_start < end_date
            query = query.Where(b => b.BookingStart < filter.EndDate.Value);
        }

        // Count before pagination
        var totalItems = await query.CountAsync();

        // 7. Apply sorting
        query = sortBy switch
        {
            "booking_start" => sortOrder == "asc"
                ? query.OrderBy(b => b.BookingStart)
                : query.OrderByDescending(b => b.BookingStart),
            "booking_end" => sortOrder == "asc"
                ? query.OrderBy(b => b.BookingEnd)
                : query.OrderByDescending(b => b.BookingEnd),
            "status" => sortOrder == "asc"
                ? query.OrderBy(b => b.Status)
                : query.OrderByDescending(b => b.Status),
            _ => sortOrder == "asc"
                ? query.OrderBy(b => b.CreatedAt)
                : query.OrderByDescending(b => b.CreatedAt)
        };

        // 8. Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

        return new PagedResult<BookingResponse>
        {
            Items = items.Select(MapToResponse),
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<BookingResponse?> UpdateAsync(Guid id, UpdateBookingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate and normalize inputs
        var borrowerName = ValidateAndNormalizeBorrowerName(request.BorrowerName);
        ValidateBookingTimes(request.BookingStart, request.BookingEnd);
        var notes = ValidateAndNormalizeNotes(request.Notes);

        var booking = await _context.Bookings.FindAsync(id);
        
        if (booking == null)
        {
            return null;
        }

        // Check for overlapping bookings (exclude current booking)
        await ValidateNoOverlap(booking.RoomId, request.BookingStart, request.BookingEnd, id);

        // RoomId cannot be changed per PRD
        // Status cannot be changed per PRD (separate endpoint)
        booking.BorrowerName = borrowerName;
        booking.BookingStart = request.BookingStart;
        booking.BookingEnd = request.BookingEnd;
        booking.Notes = notes;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
        {
            if (pgEx.ConstraintName == "chk_bookings_end_after_start")
            {
                throw new ArgumentException("Booking end time must be after start time.");
            }

            throw;
        }

        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> UpdateStatusAsync(Guid id, UpdateBookingStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate status value
        if (!Enum.TryParse<BookingStatus>(request.Status, true, out var newStatus))
        {
            throw new ArgumentException($"Invalid status value. Allowed values: Pending, Approved, Rejected.");
        }

        var booking = await _context.Bookings.FindAsync(id);
        
        if (booking == null)
        {
            return null;  // Return 404
        }

        // Update only Status (and UpdatedAt automatically via SaveChanges)
        // All other fields remain unchanged per PRD
        booking.Status = newStatus;

        await _context.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Use IgnoreQueryFilters to find even soft-deleted records
        var booking = await _context.Bookings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (booking == null || booking.DeletedAt != null)
        {
            return false;  // Not found or already soft-deleted
        }

        // Soft delete: set DeletedAt
        booking.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    // Validation helpers

    private static Guid ValidateRoomId(Guid roomId)
    {
        if (roomId == Guid.Empty)
        {
            throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));
        }

        return roomId;
    }

    private static string ValidateAndNormalizeBorrowerName(string borrowerName)
    {
        if (string.IsNullOrWhiteSpace(borrowerName))
        {
            throw new ArgumentException("BorrowerName cannot be null or whitespace.", nameof(borrowerName));
        }

        var normalized = borrowerName.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentException("BorrowerName cannot exceed 100 characters.", nameof(borrowerName));
        }

        return normalized;
    }

    private static void ValidateBookingTimes(DateTime bookingStart, DateTime bookingEnd)
    {
        // BookingEnd must be greater than BookingStart
        if (bookingEnd <= bookingStart)
        {
            throw new ArgumentException("Booking end time must be after start time.");
        }

        // BookingStart must be >= current UTC time (>= now is acceptable per PRD)
        if (bookingStart < DateTime.UtcNow)
        {
            throw new ArgumentException("Booking start time cannot be in the past.");
        }
    }

    private static string? ValidateAndNormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var normalized = notes.Trim();

        if (normalized.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        return normalized;
    }

    // Overlap detection (only Approved bookings block slots)
    // Pending/Rejected bookings can overlap - admin chooses which to approve
    private async Task ValidateNoOverlap(Guid roomId, DateTime newStart, DateTime newEnd, Guid? excludeBookingId)
    {
        // Conflict rule: (new_start < existing_end) AND (new_end > existing_start)
        // Only consider Approved bookings (Pending/Rejected don't block)
        var hasOverlap = await _context.Bookings
            .Where(b => b.RoomId == roomId)
            .Where(b => b.Status == BookingStatus.Approved)  // Only Approved blocks
            .Where(b => excludeBookingId == null || b.Id != excludeBookingId)  // Exclude current booking when updating
            .Where(b => newStart < b.BookingEnd && newEnd > b.BookingStart)  // Overlap condition
            .AnyAsync();

        if (hasOverlap)
        {
            throw new InvalidOperationException("Booking conflicts with an approved reservation.");
        }
    }

    private static BookingResponse MapToResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            BorrowerName = booking.BorrowerName,
            BookingStart = booking.BookingStart,
            BookingEnd = booking.BookingEnd,
            Notes = booking.Notes,
            Status = booking.Status.ToString(),  // Enum to string
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
            // DeletedAt NOT exposed per PRD
        };
    }
}
