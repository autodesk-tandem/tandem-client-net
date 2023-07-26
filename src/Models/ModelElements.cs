using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Models
{
    public class ModelElements
    {
        public class ModelElement
        {
            public string Key { get; set; }
            public IDictionary<string, string> Properties { get; set; }
        }

        public string Version { get; set; }
        public ModelElement[] Elements { get; set; }

    }
}
