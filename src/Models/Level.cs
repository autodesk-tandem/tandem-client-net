namespace Autodesk.Services.Tandem.Models
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
