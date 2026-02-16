namespace RoomBooking.Api.DTOs.Room;

public class UpdateRoomRequest
{
    public Guid BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
