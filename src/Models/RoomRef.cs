namespace TandemSDK.Models
{
    public class RoomRef
    {
        public string? ModelId { get; set; }
        public string? Key { get; set; }
        public string? Name { get; set; }
    }

    public interface IWithRooms
    {
        List<RoomRef> Rooms { get; }
    }
}
