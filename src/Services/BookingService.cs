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

    public async Task<PagedResult<BookingResponse>> GetAllAsync(int page, int pageSize)
    {
        // Validate and normalize pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Bookings.AsNoTracking();

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)  // Default sorting per PRD
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<BookingResponse>
        {
            Items = items.Select(MapToResponse),
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize
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

    // Overlap detection (application-level rule per PRD)
    private async Task ValidateNoOverlap(Guid roomId, DateTime newStart, DateTime newEnd, Guid? excludeBookingId)
    {
        // Conflict rule: (new_start < existing_end) AND (new_end > existing_start)
        // Consider all non-soft-deleted bookings (regardless of Status per PRD)
        var hasOverlap = await _context.Bookings
            .Where(b => b.RoomId == roomId)
            .Where(b => excludeBookingId == null || b.Id != excludeBookingId)  // Exclude current booking when updating
            .Where(b => newStart < b.BookingEnd && newEnd > b.BookingStart)  // Overlap condition
            .AnyAsync();

        if (hasOverlap)
        {
            throw new InvalidOperationException("Booking conflicts with an existing reservation.");
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
