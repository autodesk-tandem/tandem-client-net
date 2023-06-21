namespace TandemSDK.Models
{
    public class ElementBase
    {
        public ElementBase()
        {
            Flags = 0;
            Properties = new Dictionary<string, string>();
        }

        public ElementBase(ElementBase element)
        {
            Flags = element.Flags;
            Key = element.Key;
            ModelId = element.ModelId;
            Name = element.Name;
            Category = element.Category;
            CategoryId = element.CategoryId;
            Level = element.Level;
            LevelKey = element.LevelKey;
            Properties = new Dictionary<string, string>();
            foreach (var item in element.Properties)
            {
                Properties.Add(item.Key, item.Value);
            }
        }

        public long Flags{ get; set; }
        public string Key { get; set; }
        public string ModelId { get; set; }
        public string Name { get; set; }
        public long? CategoryId { get; set; }
        public string? Category { get; set; }
        public string? Level { get; set; }
        public string? LevelKey { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
