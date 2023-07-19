namespace Autodesk.Services.Tandem.Models
{
    public class FacilityClassification
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string[][] Rows { get; set; }
    }

    public interface IWithClassification
    {
        string ClassificationId { get; }
        string Classification { get; set; }
    }
}
