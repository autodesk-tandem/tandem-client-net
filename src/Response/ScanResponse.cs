using Autodesk.Services.Tandem.Models;

namespace Autodesk.Services.Tandem.Response
{
    public class ScanResponse
    {
        public string Version { get; set; }
        public ElementBase[] Items { get; set; }
    }
}
