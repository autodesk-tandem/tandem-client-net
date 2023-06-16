namespace TandemSDK.Models
{
    public class ScanResponse
    {
        public class Item
        {
            public Item()
            {
                Flags = 0;
                Properties = new Dictionary<string, string>();
            }

            public string Key { get; set; }
            public long Flags { get; set; }
            public string Name { get; set; }
            public long? CategoryId { get; set; }
            public string? Category { get; set; }
            public IDictionary<string, string> Properties { get; set; }
        }

        public string Version { get; set; }
        public Item[] Items { get; set; }
    }
}
