namespace Autodesk.Tandem.Client.Models
{
    public class Level : ElementBase
    {
        public Level(ElementBase element)
        : base(element)
        {
        }

        public double? Elevation { get; set; }
    }
}
