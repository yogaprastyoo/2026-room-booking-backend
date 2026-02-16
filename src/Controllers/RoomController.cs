using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.DTOs;
using RoomBooking.Api.DTOs.Room;
using RoomBooking.Api.DTOs.Common;
using RoomBooking.Api.Services.Interfaces;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/v1/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
    {
        try
        {
            var result = await _roomService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                StandardApiResponse<RoomResponse>.SuccessResponse(result, "Room created successfully.")
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
        var result = await _roomService.GetAllAsync(page, pageSize);
        return Ok(
            StandardApiResponse<PagedResult<RoomResponse>>.SuccessResponse(result, "Rooms retrieved successfully.")
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _roomService.GetByIdAsync(id);
        
        if (result == null)
        {
            return NotFound(
                StandardApiResponse<object>.ErrorResponse("Room not found.")
            );
        }

        return Ok(
            StandardApiResponse<RoomResponse>.SuccessResponse(result, "Room retrieved successfully.")
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomRequest request)
    {
        try
        {
            var result = await _roomService.UpdateAsync(id, request);
            
            if (result == null)
            {
                return NotFound(
                    StandardApiResponse<object>.ErrorResponse("Room not found.")
                );
            }

            return Ok(
                StandardApiResponse<RoomResponse>.SuccessResponse(result, "Room updated successfully.")
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
        var result = await _roomService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(
                StandardApiResponse<object>.ErrorResponse("Room not found.")
            );
        }

        return NoContent();
    }
}
