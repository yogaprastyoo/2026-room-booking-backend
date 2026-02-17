using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.DTOs;
using RoomBooking.Api.DTOs.Booking;
using RoomBooking.Api.DTOs.Common;
using RoomBooking.Api.Services.Interfaces;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var result = await _bookingService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                StandardApiResponse<BookingResponse>.SuccessResponse(result, "Booking created successfully.")
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
    public async Task<IActionResult> GetAll([FromQuery] BookingFilterRequest filter)
    {
        try
        {
            var result = await _bookingService.GetAllAsync(filter);
            return Ok(
                StandardApiResponse<PagedResult<BookingResponse>>.SuccessResponse(result, "Bookings retrieved successfully.")
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                StandardApiResponse<object>.ErrorResponse(ex.Message)
            );
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _bookingService.GetByIdAsync(id);
        
        if (result == null)
        {
            return NotFound(
                StandardApiResponse<object>.ErrorResponse("Booking not found.")
            );
        }

        return Ok(
            StandardApiResponse<BookingResponse>.SuccessResponse(result, "Booking retrieved successfully.")
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRequest request)
    {
        try
        {
            var result = await _bookingService.UpdateAsync(id, request);
            
            if (result == null)
            {
                return NotFound(
                    StandardApiResponse<object>.ErrorResponse("Booking not found.")
                );
            }

            return Ok(
                StandardApiResponse<BookingResponse>.SuccessResponse(result, "Booking updated successfully.")
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

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusRequest request)
    {
        try
        {
            var result = await _bookingService.UpdateStatusAsync(id, request);
            
            if (result == null)
            {
                return NotFound(
                    StandardApiResponse<object>.ErrorResponse("Booking not found.")
                );
            }

            return Ok(
                StandardApiResponse<BookingResponse>.SuccessResponse(result, "Booking status updated successfully.")
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                StandardApiResponse<object>.ErrorResponse(ex.Message)
            );
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _bookingService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(
                StandardApiResponse<object>.ErrorResponse("Booking not found.")
            );
        }

        return NoContent();
    }
}
