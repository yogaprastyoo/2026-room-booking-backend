namespace RoomBooking.Api.DTOs.Booking;

public class BookingFilterRequest
{
    // Filters
    public Guid? BuildingId { get; set; }
    public Guid? RoomId { get; set; }
    public string? Status { get; set; }
    public string? BorrowerName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Sorting
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
