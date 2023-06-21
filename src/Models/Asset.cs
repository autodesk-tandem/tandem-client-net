namespace TandemSDK.Models
{
    public class Asset : ElementBase, IWithRooms
    {
        public Asset(ElementBase element)
        : base(element)
        {
            Rooms = new List<RoomRef>();
        }

        public List<RoomRef> Rooms { get; set; }
        public string? SystemClass { get; set; }

        public IDictionary<string, string> AssetProperties { get; set; }
    }
}
