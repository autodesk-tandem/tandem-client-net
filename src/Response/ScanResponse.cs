using Autodesk.Tandem.Client.Models;

namespace Autodesk.Tandem.Client.Response
{
    public class ScanResponse
    {
        public string Version { get; set; }
        public ElementBase[] Items { get; set; }
    }
}
