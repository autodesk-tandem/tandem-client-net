namespace Autodesk.Tandem.Client.Models
{
    public class ElementBase : IWithClassification
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
            AssemblyCode = element.AssemblyCode;
            Category = element.Category;
            CategoryId = element.CategoryId;
            Classification = element.Classification;
            ClassificationId = element.ClassificationId;
            Level = element.Level;
            LevelKey = element.LevelKey;
            ParentKey = element.ParentKey;
            ParentModelId = element.ParentModelId;
            UniformatClassId = element.UniformatClassId;
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
        public string? AssemblyCode { get; set; }
        public long? CategoryId { get; set; }
        public string? Category { get; set; }
        public string? ClassificationId { get; set; }
        public string? Classification { get; set; }
        public string? Level { get; set; }
        public string? LevelKey { get; set; }
        public string? ParentKey { get; set; }
        public string? ParentModelId { get; set; }
        public string? UniformatClassId { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
