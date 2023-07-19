namespace Autodesk.Services.Tandem.Models
{
    public class Stream : ElementBase, IWithRooms
    {
        public Stream(ElementBase element)
        : base(element)
        {
            Rooms = new List<RoomRef>();
        }

        public Stream(Element element)
        : base(element)
        {
            Rooms = new List<RoomRef>();
            if (element.Rooms.Count > 0)
            {
                Rooms.AddRange(element.Rooms);
            }
        }

        public List<RoomRef> Rooms { get; set; }
    }
}
