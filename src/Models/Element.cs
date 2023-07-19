namespace Autodesk.Services.Tandem.Models
{
    public class Element : ElementBase, IWithRooms
    {
        public Element(ElementBase element)
        : base(element)
        {
            Rooms = new List<RoomRef>();
        }

        public List<RoomRef> Rooms { get; set; }
    }
}
