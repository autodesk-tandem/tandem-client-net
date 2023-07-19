namespace Autodesk.Services.Tandem.Models
{
    public class Asset : ElementBase, IWithRooms
    {
        public Asset(ElementBase element)
        : base(element)
        {
            Rooms = new List<RoomRef>();
        }

        public Asset(Element element)
        : base(element)
        {
            Rooms = new List<RoomRef>();
            if (element.Rooms.Count > 0)
            {
                Rooms.AddRange(element.Rooms);
            }
        }

        public List<RoomRef> Rooms { get; set; }
        public string? SystemClass { get; set; }
    }
}
