namespace TandemSDK.Models
{
    public class Asset
    {
        public string Key { get; set; }
        public string ModelId { get; set; }

        public string Name { get; set; }
        public string? Level { get; set; }
        public string? LevelKey { get; set; }
        public string? Room { get; set; }
        public string? RoomKey { get; set; }
        public string? SystemClass { get; set; }

        public IDictionary<string, string> AssetProperties { get; set; }
    }
}
