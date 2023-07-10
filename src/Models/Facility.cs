namespace TandemSDK.Models
{
    public class Facility
    {
        public string Id { get; set; }

        public class Document
        {
            public string Id { get; set; }
            public string ContentType { get; set; }
            public string S3Path { get; set; }
            public DateTimeOffset LastUpdated { get; set; }
            public Uri SignedLink { get; set; }
            public string Name { get; set; }
            public string AccProjectId { get; set; }
            public string AccAccountId { get; set; }
            public string AccLineage { get; set; }
            public string AccVersion { get; set; }
        }

        public class Link
        {
            public string Disciplines { get; set; }
            public string Label { get; set; }
            public bool Main { get; set; }
            public string ModelId { get; set; }
            public bool On { get; set; }
            public string AccessLevel { get; set; }
            public bool Default { get; set; } = false;
        }

        public Link[] Links { get; set; }
        public Document[] Docs { get; set; }
        public IDictionary<string, IDictionary<string, string>> Props { get; set; }
    }
}
