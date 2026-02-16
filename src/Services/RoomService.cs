using Microsoft.EntityFrameworkCore;
using Npgsql;
using RoomBooking.Api.Data;
using RoomBooking.Api.Domain;
using RoomBooking.Api.DTOs.Room;
using RoomBooking.Api.DTOs.Common;
using RoomBooking.Api.Services.Interfaces;

namespace RoomBooking.Api.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;

    public RoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoomResponse> CreateAsync(CreateRoomRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate and normalize inputs
        var buildingId = ValidateBuildingId(request.BuildingId);
        var name = ValidateAndNormalizeName(request.Name);
        var capacity = ValidateCapacity(request.Capacity);

        var room = new Room
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Name = name,
            Capacity = capacity
        };

        _context.Rooms.Add(room);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            if (pgEx.ConstraintName == "ux_rooms_building_id_name")
            {
                throw new InvalidOperationException($"Room '{name}' already exists in this building.");
            }

            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
        {
            if (pgEx.ConstraintName == "fk_rooms_building")
            {
                throw new InvalidOperationException("Building does not exist.");
            }

            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
        {
            if (pgEx.ConstraintName == "chk_rooms_capacity_positive")
            {
                throw new ArgumentException("Capacity must be greater than 0.");
            }

            throw;
        }

        return MapToResponse(room);
    }

    public async Task<RoomResponse?> GetByIdAsync(Guid id)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (room == null)
        {
            return null;
        }

        return MapToResponse(room);
    }

    public async Task<PagedResult<RoomResponse>> GetAllAsync(int page, int pageSize)
    {
        // Validate and normalize pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Rooms.AsNoTracking();

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<RoomResponse>
        {
            Items = items.Select(MapToResponse),
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<RoomResponse?> UpdateAsync(Guid id, UpdateRoomRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate and normalize inputs
        var buildingId = ValidateBuildingId(request.BuildingId);
        var name = ValidateAndNormalizeName(request.Name);
        var capacity = ValidateCapacity(request.Capacity);

        var room = await _context.Rooms.FindAsync(id);
        
        if (room == null)
        {
            return null;
        }

        room.BuildingId = buildingId;
        room.Name = name;
        room.Capacity = capacity;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            if (pgEx.ConstraintName == "ux_rooms_building_id_name")
            {
                throw new InvalidOperationException($"Room '{name}' already exists in this building.");
            }

            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
        {
            if (pgEx.ConstraintName == "fk_rooms_building")
            {
                throw new InvalidOperationException("Building does not exist.");
            }

            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
        {
            if (pgEx.ConstraintName == "chk_rooms_capacity_positive")
            {
                throw new ArgumentException("Capacity must be greater than 0.");
            }

            throw;
        }

        return MapToResponse(room);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        
        if (room == null)
        {
            return false;
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return true;
    }

    private static Guid ValidateBuildingId(Guid buildingId)
    {
        if (buildingId == Guid.Empty)
        {
            throw new ArgumentException("BuildingId cannot be empty.", nameof(buildingId));
        }

        return buildingId;
    }

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        return name.Trim();
    }

    private static int ValidateCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));
        }

        return capacity;
    }

    private static RoomResponse MapToResponse(Room room)
    {
        return new RoomResponse
        {
            Id = room.Id,
            BuildingId = room.BuildingId,
            Name = room.Name,
            Capacity = room.Capacity,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }
}
