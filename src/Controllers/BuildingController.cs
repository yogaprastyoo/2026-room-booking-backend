using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.DTOs;
using RoomBooking.Api.DTOs.Building;
using RoomBooking.Api.DTOs.Common;
using RoomBooking.Api.Services.Interfaces;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/buildings")]
public class BuildingController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBuildingRequest request)
    {
        try
        {
            var result = await _buildingService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                StandardApiResponse<BuildingResponse>.SuccessResponse(result, "Building created successfully.")
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                StandardApiResponse<object>.ErrorResponse(ex.Message)
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                StandardApiResponse<object>.ErrorResponse(ex.Message)
            );
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _buildingService.GetAllAsync(page, pageSize);
        return Ok(
            StandardApiResponse<PagedResult<BuildingResponse>>.SuccessResponse(result, "Buildings retrieved successfully.")
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _buildingService.GetByIdAsync(id);
        
        if (result == null)
        {
            return NotFound(
                StandardApiResponse<object>.ErrorResponse("Building not found.")
            );
        }

        return Ok(
            StandardApiResponse<BuildingResponse>.SuccessResponse(result, "Building retrieved successfully.")
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingRequest request)
    {
        try
        {
            var result = await _buildingService.UpdateAsync(id, request);
            
            if (result == null)
            {
                return NotFound(
                    StandardApiResponse<object>.ErrorResponse("Building not found.")
                );
            }

            return Ok(
                StandardApiResponse<BuildingResponse>.SuccessResponse(result, "Building updated successfully.")
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                StandardApiResponse<object>.ErrorResponse(ex.Message)
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                StandardApiResponse<object>.ErrorResponse(ex.Message)
            );
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _buildingService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(
                StandardApiResponse<object>.ErrorResponse("Building not found.")
            );
        }

        return NoContent();
    }
}
