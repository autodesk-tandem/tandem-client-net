namespace Autodesk.Services.Tandem.Utils
{
    internal class AssemblyCode
    {
        public class Item
        {
            public string AssemblyCode { get; set; }
            public string Description { get; set; }
            public int Level { get; set; }
            public string RevitCategory  { get; set; }
            public string Uniformat  { get; set; }
            public string Masterformat { get; set; }
            public string Notes { get; set; }
        }

        private static readonly List<Item> items = new();

        static AssemblyCode()
        {
            string[] lines = Resources.AssemblyCodeToUniformat.Split("\r\n");

            for (var i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] items = line.Split(',');

                if (items.Length < 5)
                {
                    continue;
                }
                var item = new Item
                {
                    AssemblyCode = items[0],
                    Description = items[1],
                    RevitCategory = items[3],
                    Uniformat = items[4],
                    Masterformat = items[5],
                    Notes = items[6]
                };

                if (int.TryParse(items[2], out int level))
                {
                    item.Level = level;
                    AssemblyCode.items.Add(item);
                }
            }
        }

        public static string? UniformatToAssemblyCode(string id)
        {
            var item = items.FirstOrDefault(i => string.Equals(i.Uniformat, id));

            if (item == null)
            {
                return null;
            }
            return item.Description;
		}
    }
}
