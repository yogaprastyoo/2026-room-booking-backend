using Microsoft.EntityFrameworkCore;
using Npgsql;
using RoomBooking.Api.Data;
using RoomBooking.Api.Domain;
using RoomBooking.Api.DTOs.Building;
using RoomBooking.Api.DTOs.Common;
using RoomBooking.Api.Services.Interfaces;

namespace RoomBooking.Api.Services;

public class BuildingService : IBuildingService
{
    private readonly ApplicationDbContext _context;

    public BuildingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BuildingResponse> CreateAsync(CreateBuildingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate and normalize inputs
        var name = ValidateAndNormalizeName(request.Name);
        var code = ValidateAndNormalizeCode(request.Code);

        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code
        };

        _context.Buildings.Add(building);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            if (pgEx.ConstraintName == "ux_buildings_name")
            {
                throw new InvalidOperationException($"Building with name '{name}' already exists.");
            }

            if (pgEx.ConstraintName == "ux_buildings_code")
            {
                throw new InvalidOperationException($"Building with code '{code}' already exists.");
            }

            throw;
        }

        return MapToResponse(building);
    }

    public async Task<BuildingResponse?> GetByIdAsync(Guid id)
    {
        var building = await _context.Buildings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (building == null)
        {
            return null;
        }

        return MapToResponse(building);
    }

    public async Task<PagedResult<BuildingResponse>> GetAllAsync(int page, int pageSize)
    {
        // Validate and normalize pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Buildings.AsNoTracking();

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<BuildingResponse>
        {
            Items = items.Select(MapToResponse),
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<BuildingResponse?> UpdateAsync(Guid id, UpdateBuildingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate and normalize inputs
        var name = ValidateAndNormalizeName(request.Name);
        var code = ValidateAndNormalizeCode(request.Code);

        var building = await _context.Buildings.FindAsync(id);
        
        if (building == null)
        {
            return null;
        }

        building.Name = name;
        building.Code = code;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            if (pgEx.ConstraintName == "ux_buildings_name")
            {
                throw new InvalidOperationException($"Building with name '{name}' already exists.");
            }

            if (pgEx.ConstraintName == "ux_buildings_code")
            {
                throw new InvalidOperationException($"Building with code '{code}' already exists.");
            }

            throw;
        }

        return MapToResponse(building);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var building = await _context.Buildings.FindAsync(id);
        
        if (building == null)
        {
            return false;
        }

        _context.Buildings.Remove(building);
        await _context.SaveChangesAsync();

        return true;
    }

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        return name.Trim();
    }

    private static string ValidateAndNormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    private static BuildingResponse MapToResponse(Building building)
    {
        return new BuildingResponse
        {
            Id = building.Id,
            Name = building.Name,
            Code = building.Code,
            CreatedAt = building.CreatedAt,
            UpdatedAt = building.UpdatedAt
        };
    }
}
