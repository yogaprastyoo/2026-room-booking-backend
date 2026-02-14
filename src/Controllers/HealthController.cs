using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.DTOs;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var response = StandardApiResponse<object>.SuccessResponse(
            new { status = "healthy", timestamp = DateTime.UtcNow },
            "API is running."
        );
        return Ok(response);
    }
}
