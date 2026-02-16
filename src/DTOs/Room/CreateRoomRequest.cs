namespace RoomBooking.Api.DTOs.Room;

public class CreateRoomRequest
{
    public Guid BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
